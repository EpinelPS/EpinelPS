using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/list")]
    public class ListSubquests : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSubQuestList>();

            var response = new ResGetSubQuestList();

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
