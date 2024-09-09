using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/unlockrole")]
    public class UnlockDaveRole : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUnlockDaveRole>();

            var response = new ResUnlockDaveRole
            {

            };

            await WriteDataAsync(response);
        }
    }
}
