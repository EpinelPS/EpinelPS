using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/rundispatchlist")]
public class RunDispatchList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRunDispatchList req = await ReadData<ReqRunDispatchList>();

        ResRunDispatchList response = new();

        User user = GetUser();
        DateTime startTime = DateTime.UtcNow;

        foreach (var item in req.DispatchData)
        {
            DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values
            .Where(x => x.Id == item.Tid).FirstOrDefault();

            var dispatchData = user.ResetableData.Dispatches.FirstOrDefault(x => x.TableId == item.Tid);
            if (dispatchData != null)
            {
                dispatchData.Running = true;
                dispatchData.StartAt = startTime;
                dispatchData.EndAt = startTime.AddMinutes(dispatch.TimeMin);
            }

            user.SelectableDispatchData.Add(new DispatchDataSelectable()
            {
                DispatchGroupId = dispatch.DispatchGradeId,
                Running = true,
                EndAt = startTime.AddMinutes(dispatch.TimeMin),
                SelectSlotId = item.SelectSlotId,
                SelectTid = item.Tid,
                StartAt = startTime
            });

            response.SuccessTidList.Add(item.Tid);
        }
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}