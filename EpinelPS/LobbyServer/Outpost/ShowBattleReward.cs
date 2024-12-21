using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/showoutpostbattlereward")]
    public class ShowBattleReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShowOutpostBattleReward>();
            var user = GetUser();

            var battleTime = DateTime.UtcNow - user.BattleTime;
            var battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);
            long overBattleTime = battleTimeMs > 864000000000 ? battleTimeMs - 864000000000 : 0;

            // TODO
            if (overBattleTime > 864000000000)
                overBattleTime = 0;

            var response = new ResShowOutpostBattleReward();
            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs, OverBattleTime = 0 };

            response.BattleTime = 0;
            response.FastBattleCount = user.ResetableData.WipeoutCount;
            response.MaxBattleTime = 864000000000;

            response.Reward = NetUtils.GetOutpostReward(user, battleTime);
            response.TimeRewardBuffs.AddRange(NetUtils.GetOutpostTimeReward(user));


            await WriteDataAsync(response);
        }
    }
}
