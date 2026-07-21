using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/recordlog/stageplay")]
public class RecordLogStagePlay : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordMiniGameNKSV2StagePlayLog req = await ReadData<ReqRecordMiniGameNKSV2StagePlayLog>();
        User user = GetUser();
        ResRecordMiniGameNKSV2StagePlayLog response = new();

        //Logging.WriteLine($"{req.CharacterId},{req.CharacterLevel},{req.IsResumedFromLastSave}," +
        //    $"{req.ItemCollectList},{req.MonsterKillList},{req.NKsId},{req.PlayResult},{req.StageId},{req.SurvivalTime}", LogType.Info);

        // TODO
        await WriteDataAsync(response);
    }
}