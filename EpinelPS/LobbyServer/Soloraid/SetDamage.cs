using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/setdamage")]
public class SetDamage : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        
        var req = await ReadData<ReqSetSoloRaidDamage>();
        User user = GetUser();

        ResSetSoloRaidDamage response = new();

        try
        {
            SoloRaidHelper.SetDamage(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"SetDamage Error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}