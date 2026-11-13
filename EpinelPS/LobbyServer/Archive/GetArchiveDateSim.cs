using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/minigame/DateSim/GetData")]
public class GetArchiveDateSim : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(GetArchiveDateSim));

    protected override async Task HandleAsync()
    {
        ReqGetArchiveDateSim req = await ReadData<ReqGetArchiveDateSim>();
        log.Debug($"ReqGetArchiveDateSim ArchiveDateSimId: {req.ArchiveDateSimId}");

        ResGetArchiveDateSim response = new()
        {
            ClientProgressJson = "{}",
            PermanentData = new ResGetArchiveDateSim.Types.PermanentlyStored
            {
                SpecialRewarded = false
            }
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/DateSim/FinishDialog")]
public class FinishArchiveDateSimDialog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishArchiveDateSimDialog req = await ReadData<ReqFinishArchiveDateSimDialog>();
        ResFinishArchiveDateSimDialog response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/DateSim/ObtainAlbumReward")]
public class ObtainArchiveDateSimAlbumReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainArchiveDateSimAlbumReward req = await ReadData<ReqObtainArchiveDateSimAlbumReward>();
        ResObtainArchiveDateSimAlbumReward response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/DateSim/ObtainSpecialReward")]
public class ObtainArchiveDateSimSpecialReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainArchiveDateSimSpecialReward req = await ReadData<ReqObtainArchiveDateSimSpecialReward>();
        ResObtainArchiveDateSimSpecialReward response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}
