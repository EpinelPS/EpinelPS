using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/upgradesushi")]
    public class UpgradeDaveSushi : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUpgradeDaveSushi>();


            var response = new ResUpgradeDaveSushi
            {

            };

            await WriteDataAsync(response);
        }
    }
}
