using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/showoutpostbattlereward")]
    public class ShowBattleReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqShowOutpostBattleReward req = await ReadData<ReqShowOutpostBattleReward>();
            User user = GetUser();

            TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
            long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);
            long overBattleTime = battleTimeMs > 864000000000 ? battleTimeMs - 864000000000 : 0;

            // TODO
            if (overBattleTime > 864000000000)
                overBattleTime = 0;

            ResShowOutpostBattleReward response = new()
            {
                OutpostBattleLevel = user.OutpostBattleLevel,
                OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs, OverBattleTime = 0 },

                BattleTime = 0,
                FastBattleCount = user.ResetableData.WipeoutCount,
                MaxBattleTime = 864000000000,

                Reward = NetUtils.GetOutpostReward(user, battleTime)
            };
            response.TimeRewardBuffs.AddRange(NetUtils.GetOutpostTimeReward(user));


            await WriteDataAsync(response);
        }
    }
}
