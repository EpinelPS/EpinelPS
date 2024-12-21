using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/getclearlist")]
    public class GetClearList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClearList>();

            var response = new ResGetEventMissionClearList(); //field ResGetEventMissionClearMap data type NestEventMissionClear field NestEventMissionClear data type NetEventMissionClearData fields EventId EventMissionId CreatedAt

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
