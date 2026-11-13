using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/outpost/dispatch/get")]
public class GetDispatchList : LobbyMessage
{
    // The board panel renders 11 slots. Anything above that makes the whole list render blank,
    // so the response must never exceed this.
    private const int MaxBoardSlots = 11;

    protected override async Task HandleAsync()
    {
        ReqGetDispatchList req = await ReadData<ReqGetDispatchList>();
        Random random = new Random();
        ResGetDispatchList response = new();

        User user = GetUser();
        DispatchHelper.SyncDispatchLevel(user);

        // Normal dispatches (Type 1): keep the whole board record, because the number of entries to
        // generate comes from DispatchMax and not from the daily quota in ResetableData.DispatchCount.
        DispatchBoardRecord? dispatchNormal = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.Dispatch && x.DispatchBoardLv == user.DispatchLv)
            .FirstOrDefault();
        List<DispatchBoardData> dispatch = dispatchNormal?.DispatchList ?? new List<DispatchBoardData>();
        DispatchBoardRecord? dispatchcol = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.DispatchCollection &&
                        x.DispatchBoardLv == user.DispatchCollectionLv).FirstOrDefault();
        DispatchBoardRecord? dispatchfav = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.DispatchFavorite && x.DispatchBoardLv == user.DispatchFavoriteLv)
            .FirstOrDefault();

        DateTime startTime = DateTime.UtcNow;
        int dateDay = user.GetDateDay();

        // Saves created by an older build can already hold more entries than the panel can render
        // (the daily quota of 12 was used as the number of entries to generate, plus the collection
        // and favourite boards). Those lists are replayed as-is on every later request that day, so
        // the board stays blank until the next daily reset. Rebuild them instead of serving them.
        if (user.UserDispatchData.Today != dateDay || !StoredListIsUsable(user))
        {
            user.UserDispatchData.Today = dateDay;
            user.UserDispatchData.dispatchDatas = new();
            user.DispatchClearList = new();
            user.SelectableDispatchData = new();
            user.DispatchResetCount = 0;

            List<DispatchRecord> availableDispatchTable = new List<DispatchRecord>();
            // The client keys its dispatch list by DispatchGroupId, so a group may only appear once.
            // Duplicate keys made NKUserDispatchData.UpdateListModelData index out of range.
            HashSet<int> usedGroupIds = new HashSet<int>();

            // Normal dispatches: DispatchMax (always 3) entries, not ResetableData.DispatchCount.
            // DispatchCount is the daily reset quota (12) and using it produced a large number of
            // duplicate groups.
            int normalMax = dispatchNormal?.DispatchMax ?? 0;
            for (int i = 0; i < normalMax; i++)
            {
                DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(dispatch, x => x.DispatchProb);


                List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                    .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                // Drop already picked Tids and groups that are already in use: group ids must stay unique.
                dispatchtable = dispatchtable
                    .Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                    .Where(x => !usedGroupIds.Contains(x.DispatchGroup))
                    .ToList();

                // No candidate left for this group, skip it instead of indexing an empty list.
                if (dispatchtable.Count == 0)
                    continue;

                int randomNumber = random.Next(0, dispatchtable.Count);

                DispatchRecord chosen = dispatchtable[randomNumber];
                availableDispatchTable.Add(chosen);
                usedGroupIds.Add(chosen.DispatchGroup);
                NetUserDispatchData netUserDispatch = new NetUserDispatchData
                {
                    Tid = chosen.Id,
                    IsRun = 0,
                    StartAt = startTime.Ticks,
                    EndAt = startTime.AddMinutes(chosen.TimeMin).Ticks
                };

                user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                response.DispatchList.Add(netUserDispatch);
            }

            if (dispatchcol != null)
            {
                for (int i = 0; i < dispatchcol.DispatchMax; i++)
                {
                    var list = dispatchcol.DispatchList;

                    DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(list, x => x.DispatchProb);
                    List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                        .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                    // Drop already picked Tids and groups that are already in use: group ids must stay unique.
                    dispatchtable = dispatchtable
                        .Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                        .Where(x => !usedGroupIds.Contains(x.DispatchGroup))
                        .ToList();

                    if (dispatchtable.Count == 0)
                        continue;

                    int randomNumber = random.Next(0, dispatchtable.Count);
                    DispatchRecord chosen = dispatchtable[randomNumber];
                    availableDispatchTable.Add(chosen);
                    usedGroupIds.Add(chosen.DispatchGroup);
                    NetUserDispatchData netUserDispatch = new NetUserDispatchData
                    {
                        Tid = chosen.Id,
                        IsRun = 0,
                        StartAt = startTime.Ticks,
                        EndAt = startTime.AddMinutes(chosen.TimeMin).Ticks
                    };
                    user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                    response.DispatchList.Add(netUserDispatch);
                }
            }

            if (dispatchfav != null)
            {
                for (int i = 0; i < dispatchfav.DispatchMax; i++)
                {
                    var list = dispatchfav.DispatchList;

                    DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(list, x => x.DispatchProb);
                    List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                        .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                    // Drop already picked Tids and groups that are already in use: group ids must stay unique.
                    dispatchtable = dispatchtable
                        .Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                        .Where(x => !usedGroupIds.Contains(x.DispatchGroup))
                        .ToList();

                    if (dispatchtable.Count == 0)
                        continue;

                    int randomNumber = random.Next(0, dispatchtable.Count);
                    DispatchRecord chosen = dispatchtable[randomNumber];
                    availableDispatchTable.Add(chosen);
                    usedGroupIds.Add(chosen.DispatchGroup);
                    NetUserDispatchData netUserDispatch = new NetUserDispatchData
                    {
                        Tid = chosen.Id,
                        IsRun = 0,
                        StartAt = startTime.Ticks,
                        EndAt = startTime.AddMinutes(chosen.TimeMin).Ticks
                    };
                    user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                    response.DispatchList.Add(netUserDispatch);
                }
            }
        }
        else
        {
            response.DispatchList.AddRange(user.UserDispatchData.dispatchDatas);
        }

        // Hard cap. Board levels can raise DispatchMax, and the panel still only has 11 slots.
        while (response.DispatchList.Count > MaxBoardSlots)
        {
            NetUserDispatchData extra = response.DispatchList[response.DispatchList.Count - 1];
            response.DispatchList.RemoveAt(response.DispatchList.Count - 1);
            user.UserDispatchData.dispatchDatas.Remove(extra);
        }

        // SelectableDispatchList is deliberately not sent to the client.
        // The panel header ("dispatch board X/11") shows DispatchList.Count + SelectableDispatchList.Count.
        // With nothing running the count is 6/11, but as soon as dispatches are in progress the extra
        // slot entries push it to 12/11, which is past the 11 slots the panel can render and the whole
        // list renders blank (all cards disappear, only the title and the countdowns remain).
        // The entries in DispatchList already carry IsRun / StartAt / EndAt, which is everything the
        // client needs to show a running dispatch with its countdown, so the slots are only kept in the
        // save (RunDispatch / ObtainDispatchReward still work off the Tid).
        response.DispatchResetCount = user.DispatchResetCount;
        Console.WriteLine($"[dispatch] sent {response.DispatchList.Count} dispatch entries (selectable slots withheld)");
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }

    /// <summary>
    /// Whether the entries stored for today can be served to the client as they are.
    /// The panel has 11 slots, the client keys the list by DispatchGroupId (so a group may only
    /// appear once) and every Tid must still exist in the dispatch table.
    /// </summary>
    private static bool StoredListIsUsable(User user)
    {
        List<NetUserDispatchData> stored = user.UserDispatchData.dispatchDatas;
        if (stored == null || stored.Count == 0) return false;
        if (stored.Count > MaxBoardSlots) return false;

        HashSet<int> groups = new();
        foreach (NetUserDispatchData entry in stored)
        {
            DispatchRecord? record = GameData.Instance.DispatchTable.GetValueOrDefault(entry.Tid);
            if (record == null) return false;
            if (!groups.Add(record.DispatchGroup)) return false;
        }

        return true;
    }
}