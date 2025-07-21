using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/savefield")]
    public class SaveField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSaveLostSectorField req = await ReadData<ReqSaveLostSectorField>();
            Database.User user = GetUser();

            ResSaveLostSectorField response = new();

            if (!user.LostSectorData.TryGetValue(req.SectorId, out Database.LostSectorData? value))
                user.LostSectorData.Add(req.SectorId, new Database.LostSectorData()
                {
                    Json = req.Json
                });
            else
                value.Json = req.Json;


            await WriteDataAsync(response);
        }
    }
}
