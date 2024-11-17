using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/mission/getclear")]
    public class GetClearedMissions : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClear>(); //has EventIdList
			
			
            var response = new ResGetEventMissionClear();
			// response.ResGetEventMissionClear.Add(new NetEventMissionClearData(EventId = 0, EventMissionId = 0 , CreatedAt = 0));
			
            // TODO
            await WriteDataAsync(response);
        }
    }
}
