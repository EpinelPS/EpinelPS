using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/unlocksushi")]
    public class DaveUnlockSushi : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDaveUnlockSushi>();

            var response = new ResDaveUnlockSushi
            {

            };

            await WriteDataAsync(response);
        }
    }
}
