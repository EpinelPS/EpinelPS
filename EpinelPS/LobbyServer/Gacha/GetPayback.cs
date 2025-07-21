using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha
{
    [PacketPath("/gacha/getpayback")]
    public class GetPayback : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetGachaPaybackData req = await ReadData<ReqGetGachaPaybackData>();

            ResGetGachaPaybackData response = new();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}
