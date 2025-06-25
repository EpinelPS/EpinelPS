using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/savefield")]
    public class SaveField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSaveLostSectorField>();
            var user = GetUser();

            var response = new ResSaveLostSectorField();

            if (!user.LostSectorData.ContainsKey(req.SectorId))
                user.LostSectorData.Add(req.SectorId, new Database.LostSectorData()
                {
                    Json = req.Json
                });
            else
                user.LostSectorData[req.SectorId].Json = req.Json;


            await WriteDataAsync(response);
        }
    }
}
