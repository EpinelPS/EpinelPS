using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/recordlog/unlockcharacter")]
public class RecordLogUnlockCharacter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordMiniGameNKSV2CharacterUnlockLog req = await ReadData<ReqRecordMiniGameNKSV2CharacterUnlockLog>();
        User user = GetUser();
        ResRecordMiniGameNKSV2CharacterUnlockLog response = new();

        //Logging.WriteLine($"{req.CharacterId},{req.NKsId}", LogType.Info);

        // TODO
        await WriteDataAsync(response);
    }
}