namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/jukebox/record/playhistory")]
public class JukeboxPlayHistory : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordJukeboxPlayHistory req = await ReadData<ReqRecordJukeboxPlayHistory>();

        ResRecordJukeboxPlayHistory response = new();
        await WriteDataAsync(response);
    }
}
