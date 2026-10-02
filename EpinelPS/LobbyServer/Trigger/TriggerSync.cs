using EpinelPS.Database;
using EpinelPS.LobbyServer.Mission;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.TriggerController;

[GameRequest("/trigger/sync")]
public class TriggerSync : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSyncTrigger req = await ReadData<ReqSyncTrigger>();
        User user = GetUser();

        long nowTicks = DateTime.UtcNow.Ticks;
        // Reconcile milestones on login/reset (Seq == 0) or debounced to once every 30 seconds
        // to eliminate repeated global lock contention and 11 SQLite query suites on polling ticks.
        if (req.Seq == 0 || user.NeedsTriggerSyncRestart || (nowTicks - user.LastMissionReconciledTicks) > TimeSpan.FromSeconds(30).Ticks)
        {
            MissionReconciler.ReconcileAll(user);
            user.LastMissionReconciledTicks = nowTicks;
        }
        // This request is responsible for fetching a log for
        // daily, weekly, challenge mission completion.
        // This endpoint also returns the entire "history" for the account when 
        // Seq = 0, which the client does when it is started for the first time, or when 
        // the "Clear Cache" option is invoked. 
        // When Seq = 0, the server limits the responses to 2000 items,
        // and HasRemainData is set to true.
        // TODO: Is it necessary to store the entire account history each time a stage
        // is cleared, why does the official server do this?

        ResSyncTrigger response = new();
        Logging.WriteLine($"[TriggerSync] User {user.ID} requested trigger sync from seq {req.Seq}", LogType.Debug);

        var futureTriggers = GameContext.Triggers
            .Where(x => x.UserId == user.ID && x.CreatedAt > nowTicks)
            .ToList();

        if (futureTriggers.Count > 0)
        {
            foreach (var ft in futureTriggers)
            {
                ft.CreatedAt = Math.Min(nowTicks, ft.CreatedAt - TimeSpan.FromHours(9).Ticks);
            }
            GameContext.SaveChanges();
            user.NeedsTriggerSyncRestart = true;
            Logging.WriteLine($"[TriggerSync] Corrected {futureTriggers.Count} future-dated trigger timestamps for user {user.ID}", LogType.Info);
        }

        long maxId = GameContext.Triggers
            .Where(x => x.UserId == user.ID)
            .Select(x => (long?)x.Id)
            .Max() ?? 0;

        long effectiveSeq = req.Seq;
        if (user.NeedsTriggerSyncRestart || effectiveSeq > maxId)
        {
            // The client's cached sequence is ahead of the database or triggers were sanitized.
            // Request the client to restart trigger sync from 0.
            user.NeedsTriggerSyncRestart = false;
            Logging.WriteLine($"[TriggerSync] Trigger resync needed (seq={req.Seq}, maxId={maxId}) for user {user.ID}; requesting restart from 0", LogType.Info);
            response.Restart = true;
            effectiveSeq = 0;
        }

        // Look for triggers past that amount in ascending ID order
        TriggerModelNew[] newTriggers = [.. GameContext.Triggers
            .Where(x => x.Id > effectiveSeq && x.UserId == user.ID)
            .OrderBy(x => x.Id)];

        // Return triggers (paged up to 2000)
        int triggerCount = 0;
        foreach (TriggerModelNew item in newTriggers)
        {
            triggerCount++;

            response.Triggers.Add(item.ToNet());

            if (triggerCount >= 2000)
            {
                response.HasRemainData = true;
                break;
            }
        }
        await WriteDataAsync(response);
    }
}

