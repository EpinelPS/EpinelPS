using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/get/data")]
    public class GetAzxData : LobbyMsgHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LobbyMsgHandler));
        protected override async Task HandleAsync()
        {
            // int AzxId
            ReqGetMiniGameAzxData req = await ReadData<ReqGetMiniGameAzxData>();
            User user = GetUser();

            // ResGetMiniGameAzxData Fields
            //  NetMiniGameAzxDailyMissionData DailyMissionData
            //  RepeatedField<NetMiniGameAzxAchievementMissionData> AchievementMissionDataList
            //  RepeatedField<NetMiniGameAzxCutSceneData> CutSceneList
            //  int SelectedBoardId
            //  int SelectedCharacterId
            //  RepeatedField<NetMiniGameAzxConditionalBoardData> ConditionalBoardDataList
            //  RepeatedField<NetMiniGameAzxConditionalCharacterData> ConditionalCharacterDataList
            //  RepeatedField<NetMiniGameAzxConditionalSkillData> ConditionalSkillDataList
            //  bool IsTutorialConfirmed
            ResGetMiniGameAzxData response = new()
            {
                DailyMissionData = new NetMiniGameAzxDailyMissionData()
                {
                    DailyAccumulatedScore = 0,
                    IsDailyRewarded = false,
                },
            };

            // TODO: Add implementation for AchievementMissionDataList, CutSceneList, etc.
            AzxHelper.GetAzxData(user, req.AzxId, ref response);


            await WriteDataAsync(response);
        }
    }
}