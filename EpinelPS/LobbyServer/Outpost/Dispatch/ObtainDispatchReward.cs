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
        User user = GetUser();
        NetRewardData rewardData = new();
        var dispatchDatas = user.ResetableData.Dispatches;

        foreach (var item in req.TidList)
        {
            DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values.Where(c => c.Id == item).FirstOrDefault();
            int rewardid = dispatch.RewardId;
            PassHelper.RewardsForUser(user, ref rewardData, rewardid);
            user.AddUnique(user.DispatchClearList, item);
            user.ResetableData.Dispatches.RemoveAll(x => x.TableId == item);

            user.AddTrigger(Trigger.SendDispatch, 1, 0);
            user.AddTrigger(Trigger.SendDispatchGrade, 1, dispatch.DispatchGradeId);
        }


        //List<NetSelectableDispatchData> dispatchtable = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        response.Reward = rewardData;
        foreach (var item in user.ResetableData.Dispatches)
            response.DispatchList.Add(item.ToNet());

        await GameContext.SaveChangesAsync();
        await WriteDataAsync(response);
    }

}