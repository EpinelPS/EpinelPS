using System;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Guild;

[GameRequest("/guild/unionraid/get")]
public class UnionRaidGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidData>();
        DateTime now = DateTime.UtcNow;

        ResGetUnionRaidData response = new()
        {
            Data = new NetUnionRaidData
            {
                UnionRaidState = UnionRaidState.Ready,
                UnionRaidManagerTid = 1000001,
                TotalRank = 999999,
                NormalClearDay = 0,
                CurrentStatus = new NetUnionRaidGuildInfo
                {
                    Gsn = 10001,
                    AchieveLevel = 1,
                    AchieveStep = 1,
                    LeftHp = 100000000
                },
                UnionRaidJoinData = new NetUnionRaidJoinData
                {
                    PlayCount = 0,
                    HardPlayCount = 0
                },
                PeriodData = new NetUnionRaidPeriodData
                {
                    VisibleDate = now.AddDays(-1).Ticks,
                    StartDate = now.AddDays(7).Ticks,
                    EndDate = now.AddDays(14).Ticks,
                    SettleDate = now.AddDays(15).Ticks,
                    DisableDate = now.AddDays(21).Ticks
                },
                UnionRaidRanking = new NetUnionRaidRankInfo
                {
                    Ranking = 999999,
                    TotalDamage = 0,
                    NormalClearDay = 0
                }
            }
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/getstepinfo")]
public class UnionRaidStepInfoGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidStepInfo>();
        ResGetUnionRaidStepInfo response = new()
        {
            Info = new NetUnionRaidGuildInfo
            {
                Gsn = 10001,
                AchieveLevel = 1,
                AchieveStep = 1,
                LeftHp = 100000000
            },
            PeriodResult = UnionRaidPeriodResult.Success,
            NormalClearDay = 0
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/get/nowplaying")]
public class UnionRaidNowPlayingGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidNowPlaying>();
        ResGetUnionRaidNowPlaying response = new()
        {
            NowPlaying = false,
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/getjoindata")]
public class UnionRaidJoinDataGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidJoinData>();
        ResGetUnionRaidJoinData response = new()
        {
            UnionRaidJoinData = new NetUnionRaidJoinData
            {
                PlayCount = 0,
                HardPlayCount = 0
            }
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/hard/getstepinfo")]
public class UnionRaidHardStepInfoGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidHardStepInfo>();
        ResGetUnionRaidHardStepInfo response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/hard/get/nowplaying")]
public class UnionRaidHardNowPlayingGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidHardNowPlaying>();
        ResGetUnionRaidHardNowPlaying response = new()
        {
            NowPlaying = false,
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/bosspopup/get")]
public class UnionRaidBossPopupGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidBossPopupData>();
        ResGetUnionRaidBossPopupData response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/bosspopup/set")]
public class UnionRaidBossPopupSet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetUnionRaidBossPopupData>();
        ResSetUnionRaidBossPopupData response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/memberlog")]
public class UnionRaidMemberLogGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidMemberLog>();
        ResGetUnionRaidMemberLog response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/log/step")]
public class UnionRaidStepBattleLogGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidStepBattleLog>();
        ResGetUnionRaidStepBattleLog response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/log/user")]
public class UnionRaidUserBattleLogGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidUserBattleLog>();
        ResGetUnionRaidUserBattleLog response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/log/whole")]
public class UnionRaidWholeBattleLogGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetUnionRaidWholeBattleLog>();
        ResGetUnionRaidWholeBattleLog response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/enter")]
public class UnionRaidEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEnterUnionRaid>();
        ResEnterUnionRaid response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/setdamage")]
public class UnionRaidSetDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetUnionRaidDamage>();
        ResSetUnionRaidDamage response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/practice/enter")]
public class UnionRaidPracticeEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEnterUnionRaidPractice>();
        ResEnterUnionRaidPractice response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/pratice/setdamage")]
public class UnionRaidPracticeSetDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetUnionRaidPracticeDamage>();
        ResSetUnionRaidPracticeDamage response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/hard/enter")]
public class UnionRaidHardEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEnterUnionRaidHard>();
        ResEnterUnionRaidHard response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/hard/setdamage")]
public class UnionRaidHardSetDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetUnionRaidHardDamage>();
        ResSetUnionRaidHardDamage response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/hard/practice/enter")]
public class UnionRaidHardPracticeEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEnterUnionRaidHardPractice>();
        ResEnterUnionRaidHardPractice response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/pratice/setdamage/hard")]
public class UnionRaidHardPracticeSetDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetUnionRaidHardPracticeDamage>();
        ResSetUnionRaidHardPracticeDamage response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/unionraid/ranking/world")]
public class UnionRaidWorldRankingList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqListUnionRaidWorldRanking>();
        ResListUnionRaidWorldRanking response = new()
        {
            PeriodResult = UnionRaidPeriodResult.Success
        };
        await WriteDataAsync(response);
    }
}
