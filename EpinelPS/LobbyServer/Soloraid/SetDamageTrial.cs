using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/trial/setdamage")]
public class SetDamageTrial : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        
        var req = await ReadData<ReqSetSoloRaidTrialDamage>();
        User user = GetUser();

        ResSetSoloRaidTrialDamage response = new();

        try
        {
            SoloRaidHelper.SetDamageTrial(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"SetDamageTrial Error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}