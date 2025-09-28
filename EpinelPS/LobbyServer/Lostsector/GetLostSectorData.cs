using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/get")]
    public class GetLostSectorData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetLostSectorData req = await ReadData<ReqGetLostSectorData>();
            User user = GetUser();

            ResGetLostSectorData response = new();

            foreach (KeyValuePair<int, LostSectorRecord> item in GameData.Instance.LostSector)
            {
                if (item.Value.OpenConditionType == ContentOpenType.Stage && user.IsStageCompleted(item.Value.OpenConditionValue))
                {
                    response.ClearStages.Add(new NetFieldStageData() { StageId = item.Value.OpenConditionValue });
                }

                if (user.LostSectorData.TryGetValue(item.Key, out LostSectorData? val))
                {
                    var map = GameData.Instance.MapData[item.Value.FieldId];
                    response.LostSector.Add(new NetUserLostSectorData()
                    {
                        IsOpen = val.IsOpen,
                        SectorId = item.Key,
                        IsPlaying = val.IsPlaying,
                        CurrentClearStageCount = val.ClearedStages.Count,
                        RewardCount = val.ObtainedRewards,
                        IsFinalReward = val.RecievedFinalReward,
                        IsPerfectReward = val.CompletedPerfectly,
                        MaxClearStageCount = map.StageSpawner.Count
                    });
                }
            }

            await WriteDataAsync(response);
        }
    }
}
