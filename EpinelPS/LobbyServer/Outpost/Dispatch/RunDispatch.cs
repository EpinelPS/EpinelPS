using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/rundispatch")]
public class RunDispatch : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRunDispatch req = await ReadData<ReqRunDispatch>();

        ResRunDispatch response = new();

        User user = GetUser();
        DateTime startTime = DateTime.UtcNow;

        DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values
            .Where(x => x.Id == req.Tid).FirstOrDefault();
        
        response.DispatchData = new()
        {
            Tid = req.Tid,
            IsRun = 1,
            StartAt = startTime.Ticks,
            EndAt = startTime.AddMinutes(dispatch.TimeMin).Ticks
        };

        var dispatchData = user.ResetableData.Dispatches.FirstOrDefault(x => x.TableId == req.Tid);
        if (dispatchData != null)
        {
            dispatchData.Running = true; 
            dispatchData.StartAt = startTime;
            dispatchData.EndAt = startTime.AddMinutes(dispatch.TimeMin);
            //dispatchData.EndAt = startTime.AddSeconds(dispatch.TimeMin).Ticks;
        }

        user.SelectableDispatchData.Add(new DispatchDataSelectable()
        {
            DispatchGroupId = dispatch.DispatchGradeId,
            Running = true,
            EndAt = startTime.AddMinutes(dispatch.TimeMin),
            SelectSlotId = req.SelectSlotId,
            SelectTid = req.Tid,
            StartAt = startTime
        });

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}