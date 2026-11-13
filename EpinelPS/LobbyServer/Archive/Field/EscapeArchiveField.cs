using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/escape")]
public class EscapeArchiveField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqEscapeArchiveField>();
        ResEscapeArchiveField response = new();
        await WriteDataAsync(response);
    }
}
