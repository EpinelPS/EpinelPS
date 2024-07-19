using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/support-character/list-used-count")]
    public class ListSupportCharacterCount : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListSupportCharacterUsedCount>();

            var response = new ResListSupportCharacterUsedCount();

            // TODO: Limit temportary participation
            foreach (var item in req.TeamTypeList)
            {
                Console.WriteLine("support character used: " + item);
            }
            await WriteDataAsync(response);
        }
    }
}
