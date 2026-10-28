using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Event.Minigame.DateSim;

[GameRequest("/minigame/datesim/get")]
public class DateSimGet : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(DateSimGet));

    protected override async Task HandleAsync()
    {
        ReqGetDateSim req = await ReadData<ReqGetDateSim>();
        log.Debug($"ReqGetDateSim DateSimId: {req.DateSimId}");

        ResGetDateSim response = new()
        {
            DateSimJson = "{}",
            Stamina = new NetDateSimStaminaData
            {
                InfiniteMode = true,
                Stamina = 100,
                TodayUsedStamina = 0
            },
            SpecialRewarded = false
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/minigame/datesim/findialog")]
public class DateSimFinDialog : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(DateSimFinDialog));

    protected override async Task HandleAsync()
    {
        ReqFinDateSimDialog req = await ReadData<ReqFinDateSimDialog>();
        log.Debug($"ReqFinDateSimDialog DateSimId: {req.DateSimId}, DialogId: {req.DialogId}");

        ResFinDateSimDialog response = new()
        {
            StaminaData = new NetDateSimStaminaData
            {
                InfiniteMode = true,
                Stamina = 100,
                TodayUsedStamina = 0
            },
            DailyReward = new NetRewardData()
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/minigame/datesim/albumreward")]
public class DateSimAlbumReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainDateSimAlbumReward req = await ReadData<ReqObtainDateSimAlbumReward>();
        ResObtainDateSimAlbumReward response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/minigame/datesim/specialreward")]
public class DateSimSpecialReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainDateSimSpecialReward req = await ReadData<ReqObtainDateSimSpecialReward>();
        ResObtainDateSimSpecialReward response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/minigame/datesim/recordheroinerelation")]
public class DateSimRecordHeroineRelation : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordDateSimHeroineRelation req = await ReadData<ReqRecordDateSimHeroineRelation>();
        ResRecordDateSimHeroineRelation response = new();
        await WriteDataAsync(response);
    }
}
