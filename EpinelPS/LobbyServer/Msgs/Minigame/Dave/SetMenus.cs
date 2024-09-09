using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/setmenus")]
    public class SetDaveSushiMenus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetDaveSushiMenus>();

            var response = new ResSetDaveSushiMenus
            {

            };

            await WriteDataAsync(response);
        }
    }
}
