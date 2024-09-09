using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/changemasterrole")]
    public class ChangeDaveMasterRole : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqChangeDaveMasterRole>();

            var response = new ResChangeDaveMasterRole
            {

            };

            await WriteDataAsync(response);
        }
    }
}
