using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/event/mission/getclear")]
    public class GetClearedMissions : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClear>();

            var response = new ResGetEventMissionClear();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
