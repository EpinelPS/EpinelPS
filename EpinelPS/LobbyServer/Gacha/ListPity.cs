using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha
{
    [PacketPath("/gacha/pity/list")]
    public class ListPity : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListGachaPityProgress req = await ReadData<ReqListGachaPityProgress>();

            ResListGachaPityProgress response = new();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}
