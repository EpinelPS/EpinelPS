using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/practice/setdamage")]
public class SetDamagePractice : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSetSoloRaidPracticeDamage>();
        var user = GetUser();
        ResSetSoloRaidPracticeDamage response = new();

        try
        {
            SoloRaidHelper.SetDamagePractice(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"SetDamagePractice Error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}