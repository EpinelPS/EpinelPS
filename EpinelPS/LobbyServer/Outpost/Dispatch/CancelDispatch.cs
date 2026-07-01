using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/outpost/canceldispatch")]
public class CancelDispatch : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCancelDispatch req = await ReadData<ReqCancelDispatch>();
        ResCancelDispatch response = new();
        User user = GetUser();
        DateTime startTime = DateTime.UtcNow;

        DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values
            .Where(x => x.Id == req.Tid).FirstOrDefault();
        
        
        var dispatchData = user.UserDispatchData.dispatchDatas.FirstOrDefault(x => x.Tid == req.Tid);
        if (dispatchData != null)
        {
            dispatchData.IsRun = 0;
            dispatchData.StartAt = startTime.Ticks;
            dispatchData.EndAt = startTime.AddMinutes(dispatch.TimeMin).Ticks;
        }

        user.SelectableDispatchData.RemoveAll(cs => cs.SelectTid == req.Tid);

        JsonDb.Save();           

        // TODO
        await WriteDataAsync(response);
    }
}
