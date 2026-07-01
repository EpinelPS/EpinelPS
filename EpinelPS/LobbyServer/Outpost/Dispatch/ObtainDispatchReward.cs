using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Pass;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/outpost/obtaindispatchreward")]
public class ObtainDispatchReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainDispatchReward req = await ReadData<ReqObtainDispatchReward>();

        ResObtainDispatchReward response = new();
        //Logging.WriteLine($"获取 {req.TidList}", LogType.Info);
        User user = GetUser();
        NetRewardData rewardData = new();
        List<NetUserDispatchData> dispatchDatas = user.UserDispatchData.dispatchDatas;

        foreach (var item in req.TidList)
        {
            DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values.Where(c => c.Id == item).FirstOrDefault();
            int rewardid = dispatch.RewardId;                       
            PassHelper.RewardsForUser(user, ref rewardData, rewardid);
            user.AddUnique(user.DispatchClearList, item);
            user.UserDispatchData.dispatchDatas.RemoveAll(x => x.Tid == item);
            dispatchDatas.RemoveAll(x => x.Tid == item);

            user.AddTrigger(Trigger.SendDispatch, 1, 0);
            user.AddTrigger(Trigger.SendDispatchGrade, 1, dispatch.DispatchGradeId);
        }


        List<NetSelectableDispatchData> dispatchtable = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        response.Reward = rewardData;
        response.DispatchList.AddRange(dispatchDatas);
        response.SelectableDispatchList.AddRange(dispatchtable);

        JsonDb.Save();
         // TODO
         await WriteDataAsync(response);
    }

}