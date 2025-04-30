using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha
{
    [PacketPath("/gacha/getpayback")]
    public class GetPayback : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetGachaPaybackData>();

            var response = new ResGetGachaPaybackData();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}
