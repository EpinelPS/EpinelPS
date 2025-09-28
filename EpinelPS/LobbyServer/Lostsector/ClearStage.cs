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
            // get lost sector Id from stage Id
            int sector = GameData.Instance.LostSectorStages[stageId].Sector;

            // get position ID from stage Id in map data

            LostSectorRecord sectorData = GameData.Instance.LostSector[sector];
            var mapInfo = GameData.Instance.MapData[sectorData.FieldId];

            var stage = mapInfo.StageSpawner.Where(x => x.StageId == stageId).FirstOrDefault() ?? throw new Exception("cannot find stage in map data");

            user.LostSectorData[sector].ClearedStages.Add(stage.PositionId, stageId);
        }
    }
}
