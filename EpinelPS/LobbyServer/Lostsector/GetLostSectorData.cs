using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/get")]
    public class GetLostSectorData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLostSectorData>();
            var user = GetUser();

            var response = new ResGetLostSectorData();

            foreach (var item in GameData.Instance.LostSector)
            {
                if (item.Value.open_condition_type == ContentOpenType.Stage && user.IsStageCompleted(item.Value.open_condition_value))
                {
                    response.ClearStages.Add(new NetFieldStageData() { StageId = item.Value.open_condition_value });
                }

                if (user.LostSectorData.ContainsKey(item.Key))
                {
                    var map = GameData.Instance.MapData[item.Value.field_id];
                    var val = user.LostSectorData[item.Key];
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
