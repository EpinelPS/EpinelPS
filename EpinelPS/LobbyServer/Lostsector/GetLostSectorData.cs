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
            Database.User user = GetUser();

            ResGetLostSectorData response = new();

            foreach (KeyValuePair<int, LostSectorRecord> item in GameData.Instance.LostSector)
            {
                if (item.Value.open_condition_type == ContentOpenType.Stage && user.IsStageCompleted(item.Value.open_condition_value))
                {
                    response.ClearStages.Add(new NetFieldStageData() { StageId = item.Value.open_condition_value });
                }

                if (user.LostSectorData.TryGetValue(item.Key, out Database.LostSectorData? val))
                {
                    MapInfo map = GameData.Instance.MapData[item.Value.field_id];
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
