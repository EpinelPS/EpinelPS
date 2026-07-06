using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/finish")]
public class Finish : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishArcadeTowerDefense req = await ReadData<ReqFinishArcadeTowerDefense>();
        User user = GetUser();
        ResFinishArcadeTowerDefense response = new();

        NetRewardData ret = new() { PassPoint = new() };

        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            data.ClearedTutorialIdList.AddRangeUnique(req.TutorialIds);

            if (req.Score > data.ChallengeMaxScore)
            {
                data.ChallengeMaxScore = req.Score;

                if (user.Guild.guildId > 0)
                {
                    MiniGameHelper.InsertOrUpdate(req.ArcadeManagerId, user.ID, user.Guild.guildId.Value, req.Score, 0);
                }               
            }

            if (req.IsWin)
            {
                if (!data.ClearedStageIdList.Contains(req.StageId))
                {
                    data.ClearedStageIdList.AddUnique(req.StageId);
                    EventTowerDefenseStageRecord? clear = GameData.Instance.EventTowerDefenseStageTable.Values
                        .Where(x => x.Id == req.StageId).FirstOrDefault();
                    if (clear.StageFirstClearReward > 0)
                    {
                        ret = RewardUtils.RegisterRewardsForUser(user, clear.StageFirstClearReward);
                        data.UpgradeCurrency += clear.StageFirstClearUpgradeCurrencyReward;
                        response.UpgradeCurrencyAmount = clear.StageFirstClearUpgradeCurrencyReward;
                    }

                }

            }
            else
            {
                response.UpgradeCurrencyAmount = 0;
            }

            response.CurrentUpgradeCurrency = data.UpgradeCurrency;
            
        }

        response.Reward = ret;
        

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}