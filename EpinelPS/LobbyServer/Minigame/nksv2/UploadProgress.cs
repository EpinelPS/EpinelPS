using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/uploadprogress")]
public class UploadProgress : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUploadMiniGameNKSV2Progress req = await ReadData<ReqUploadMiniGameNKSV2Progress>();
        User user = GetUser();
        ResUploadMiniGameNKSV2Progress response = new() { BanResult = MiniGameBanResult.Success};

        //Logging.WriteLine($"{req.NKsId},{req.ProgressJson}", LogType.Info);

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            nksv2Data.ProgressJson = req.ProgressJson;
            response.BanResult = MiniGameBanResult.Success;
            response.Error = NKSV2MissionExpiredError.Succeed;
        }
       

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}