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

        //Logging.WriteLine($"获取 {req}", LogType.Info);
        User user = GetUser();
        DateTime startTime = DateTime.UtcNow;

        foreach (var item in req.DispatchData)
        {
            DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values
            .Where(x => x.Id == item.Tid).FirstOrDefault();                

            var dispatchData = user.UserDispatchData.dispatchDatas.FirstOrDefault(x => x.Tid == item.Tid);
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
                SelectSlotId = item.SelectSlotId,
                SelectTid = item.Tid,
                StartAt = startTime.Ticks
            });

            response.SuccessTidList.Add(item.Tid);
        }
                
        

        JsonDb.Save();



        // TODO
        await WriteDataAsync(response);
    }
}