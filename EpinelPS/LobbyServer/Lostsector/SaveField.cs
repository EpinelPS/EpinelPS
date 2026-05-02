namespace EpinelPS.LobbyServer.Lostsector;

[GameRequest("/lostsector/savefield")]
public class SaveField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveLostSectorField req = await ReadData<ReqSaveLostSectorField>();
        User user = GetUser();

        ResSaveLostSectorField response = new();

        if (!user.LostSectorData.TryGetValue(req.SectorId, out LostSectorData? value))
            user.LostSectorData.Add(req.SectorId, new LostSectorData()
            {
                Json = req.Json
            });
        else
            value.Json = req.Json;


        await WriteDataAsync(response);
    }
}
