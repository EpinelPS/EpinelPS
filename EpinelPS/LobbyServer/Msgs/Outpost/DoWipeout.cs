using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/obtainfastbattlereward")]
    public class DoWipeout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainFastBattleReward>();
            var response = new ResObtainFastBattleReward();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
