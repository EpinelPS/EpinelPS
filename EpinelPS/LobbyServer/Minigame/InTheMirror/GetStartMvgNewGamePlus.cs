using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/newgameplus")]
public class GetStartMvgNewGamePlus : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var request = await ReadData<ReqStartArcadeMvgNewGamePlus>();

        var user = GetUser();

        foreach (var questData in user.ArcadeInTheMirrorData.Quests.Where(q => q.IsReceived == false))
        {
            var quest = GameData.Instance.EventMvgQuestTable[questData.QuestId];
            if (quest.RewardId != 0)
            {
                foreach (var rewardEntry in GameData.Instance.RewardDataRecords[quest.RewardId].Rewards ??= [])
                {
                    if (rewardEntry.RewardType != RewardType.None)
                    {
                        switch (rewardEntry.RewardId)
                        {
                            case 9810003:
                                user.ArcadeInTheMirrorData.Gold += rewardEntry.RewardValue;
                                break;
                            case 9811001:
                                user.ArcadeInTheMirrorData.Core += rewardEntry.RewardValue;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        foreach (var quest in user.ArcadeInTheMirrorData.Quests)
        {
            quest.Progress = default;
            quest.IsReceived = default;
        }

        user.ArcadeInTheMirrorData.ProgressJson = request.ProgressJson;

        await WriteDataAsync(new ResStartArcadeMvgNewGamePlus());

        JsonDb.Save();
    }
}
