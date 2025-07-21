using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Team
{
    [PacketPath("/team/support-character/list-used-count")]
    public class ListSupportCharacterCount : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListSupportCharacterUsedCount req = await ReadData<ReqListSupportCharacterUsedCount>();

            ResListSupportCharacterUsedCount response = new();

            // TODO: Limit temportary participation
            foreach (int item in req.TeamTypeList)
            {
                Console.WriteLine("support character used: " + item);
            }
            await WriteDataAsync(response);
        }
    }
}
