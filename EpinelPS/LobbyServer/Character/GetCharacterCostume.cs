using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/costume/get")]
public class GetCharacterCostume : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetCharacterCostumeData req = await ReadData<ReqGetCharacterCostumeData>();

        ResGetCharacterCostumeData response = new();

        // return all
        response.CostumeIds.AddRange(GameData.Instance.GetAllCostumes());

        await WriteDataAsync(response);
    }
}
