using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/recordlog/skilltreeupgrade")]
public class RecordLogSkillTreeUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordMiniGameNKSV2SkillTreeUpgradeLog req = await ReadData<ReqRecordMiniGameNKSV2SkillTreeUpgradeLog>();
        User user = GetUser();
        ResRecordMiniGameNKSV2SkillTreeUpgradeLog response = new();

        //Logging.WriteLine($"{req.ActType},{req.CharacterId},{req.NKsId},{req.SkillTreeId},{req.SlotItemId}", LogType.Info);

        // TODO
        await WriteDataAsync(response);
    }
}