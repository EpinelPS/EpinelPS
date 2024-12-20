using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/usepiece")]
    public class UsePiece : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUsePiece>();
            var user = GetUser();

            var response = new ResUsePiece();
            
            // TODO

            await WriteDataAsync(response);
        }
    }
}
