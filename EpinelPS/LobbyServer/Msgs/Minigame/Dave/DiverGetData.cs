using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/getdata")]
    public class GetDaveData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetDaveData>();

            var response = new ResGetDaveData
            {
				
            };

            await WriteDataAsync(response);
        }
    }
}
