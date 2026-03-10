using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/boxgacha/get")]
    public class GetEventBoxGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // from client: {"EventId":10051}
            ReqGetEventBoxGacha req = await ReadData<ReqGetEventBoxGacha>();
            User user = User;

            ResGetEventBoxGacha response = new()
            {
                
            };

            await WriteDataAsync(response);
        }
    }
}