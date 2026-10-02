namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/system/battlelog/disallowed-team-types")]
public class GetBattleLogDisallowedTeamTypes : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetBattleLogUploadDisallowedTeamTypes req = await ReadData<ReqGetBattleLogUploadDisallowedTeamTypes>();

        ResGetBattleLogUploadDisallowedTeamTypes response = new();

        await WriteDataAsync(response);
    }
}
