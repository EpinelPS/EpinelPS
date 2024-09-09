using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/obtainmissionreward")]
    public class ObtainMiniGameDaveMissionReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainMiniGameDaveMissionReward>();

            var response = new ResObtainMiniGameDaveMissionReward
            {

            };

            await WriteDataAsync(response);
        }
    }
}
