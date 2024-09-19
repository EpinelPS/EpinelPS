using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event.Mission
{
    [PacketPath("/event/mission/getclearlist")]
    public class GetClearList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClearList>();

            var response = new ResGetEventMissionClearList();

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
