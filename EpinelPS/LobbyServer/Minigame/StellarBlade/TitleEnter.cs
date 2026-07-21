using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/title/enter")]
public class TitleEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeEnterStellarBladeTitle req = await ReadData<ReqArcadeEnterStellarBladeTitle>();
        User user = GetUser();
        ResArcadeEnterStellarBladeTitle response = new();
        
        Logging.WriteLine($"{req.ArcadeManagerId}", LogType.Info);

        NetMiniGameStellarBladeRankingData? rank = MiniGameHelper.GetSBUserRank(user.ID, req.ArcadeManagerId);

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            if (rank == null)
            {
                response.UserRankingData = new() 
                { Rank = 0, Score = 0, User = LobbyHandler.CreateWholeUserDataFromDbUser((ulong)user.ID) };
            }
            else
            {
                response.UserRankingData = rank;
            }
            
            response.TutorialListTableIds.AddRange(stellar.TutorialList);
            response.SbItemIdList.AddRange(stellar.SbItemIdList);
            response.LastEnteredStageId = stellar.LastEnteredStageId;
            response.CharacterData = stellar.CharacterData.ToProto();
            var currlist = MiniGameHelper.ToProtoList<NetStellarBladeCurrency, StellarBladeCurrency>(stellar.Currency);
            response.CurrencyList.AddRange(currlist);
            var misslist = MiniGameHelper.ToProtoList<NetStellarBladeMissionData, StellarBladeMissionData>(stellar.MissionData);
            response.AchievementMissionDataList.AddRange(misslist);             
            
        }

        // TODO
        await WriteDataAsync(response);
    }
}