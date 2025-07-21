using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/clearstage")]
    public class ClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearLostSectorStage req = await ReadData<ReqClearLostSectorStage>();
            User user = GetUser();

            ResClearLostSectorStage response = new();

            if (req.BattleResult == 1)
            {
                ClearLostSectorStage(user, req.StageId);
                JsonDb.Save();
            }

            await WriteDataAsync(response);
        }

        public static void ClearLostSectorStage(User user, int stageId)
        {
            // get lost sector id from stage id
            int sector = GameData.Instance.LostSectorStages[stageId].sector;

            // get position ID from stage id in map data

            LostSectorRecord sectorData = GameData.Instance.LostSector[sector];
            MapInfo mapInfo = GameData.Instance.MapData[sectorData.field_id];

            StageSpawner stage = mapInfo.StageSpawner.Where(x => x.stageId == stageId).FirstOrDefault() ?? throw new Exception("cannot find stage in map data");

            user.LostSectorData[sector].ClearedStages.Add(stage.positionId, stageId);
        }
    }
}
