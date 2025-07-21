using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/getclearlist")]
    public class GetClearList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetEventMissionClearList req = await ReadData<ReqGetEventMissionClearList>();

            ResGetEventMissionClearList response = new(); //field ResGetEventMissionClearMap data type NestEventMissionClear field NestEventMissionClear data type NetEventMissionClearData fields EventId EventMissionId CreatedAt

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
