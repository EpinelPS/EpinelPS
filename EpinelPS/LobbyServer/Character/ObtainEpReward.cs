using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/obtainreward")]
    public class ObtainEpReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainAttractiveReward>();
            var response = new ResObtainAttractiveReward();
            
            // TODO

            await WriteDataAsync(response);
        }
    }
}
