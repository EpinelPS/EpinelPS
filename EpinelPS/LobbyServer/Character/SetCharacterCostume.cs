using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/costume/set")]
public class SetCharacterCostume : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetCharacterCostume req = await ReadData<ReqSetCharacterCostume>();
        User user = GetUser();

        foreach (CharacterModel item in user.Characters)
        {
            if (item.Csn == req.Csn)
            {
                item.CostumeId = req.CostumeId;
                break;
            }
        }
        JsonDb.Save();

        ResSetCharacterCostume response = new();

        await WriteDataAsync(response);
    }
}
