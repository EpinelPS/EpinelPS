using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha
{
    [PacketPath("/gacha/pity/list")]
    public class ListPity : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListGachaPityProgress>();

            var response = new ResListGachaPityProgress();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}
