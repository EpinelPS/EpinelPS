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

        //Logging.WriteLine($"获取 {req}:{req.SelectSlotId},{req.Tid},{req.CsnList}", LogType.Info);
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

        var dispatchData = user.UserDispatchData.dispatchDatas.FirstOrDefault(x => x.Tid == req.Tid);
        if (dispatchData != null)
        {
            dispatchData.IsRun = 1; 
            dispatchData.StartAt = startTime.Ticks;
            dispatchData.EndAt = startTime.AddMinutes(dispatch.TimeMin).Ticks;
            //dispatchData.EndAt = startTime.AddSeconds(dispatch.TimeMin).Ticks;
        }

        user.SelectableDispatchData.Add(new NetSelectableDispatchData()
        {
            DispatchGroupId = dispatch.DispatchGradeId,
            IsRun = true,
            EndAt = startTime.AddMinutes(dispatch.TimeMin).Ticks,
            //EndAt = startTime.AddSeconds(dispatch.TimeMin).Ticks,
            SelectSlotId = req.SelectSlotId,
            SelectTid = req.Tid,
            StartAt = startTime.Ticks
        });

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}