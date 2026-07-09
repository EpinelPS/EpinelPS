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
        
        var dispatchData = user.ResetableData.Dispatches.FirstOrDefault(x => x.TableId == req.Tid);
        if (dispatchData != null)
        {
            dispatchData.Running = false;
            dispatchData.StartAt = startTime;
            dispatchData.EndAt = startTime.AddMinutes(dispatch.TimeMin);
        }

        user.SelectableDispatchData.RemoveAll(cs => cs.SelectTid == req.Tid);

        await GameContext.SaveChangesAsync();       

        await WriteDataAsync(response);
    }
}
