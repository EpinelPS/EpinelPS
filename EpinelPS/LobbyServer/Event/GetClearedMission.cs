using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/mission/getclear")]
    public class GetClearedMissions : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetEventMissionClear req = await ReadData<ReqGetEventMissionClear>(); //has EventIdList


            ResGetEventMissionClear response = new();
			// response.ResGetEventMissionClear.Add(new NetEventMissionClearData(EventId = 0, EventMissionId = 0 , CreatedAt = 0));
			
            // TODO
            await WriteDataAsync(response);
        }
    }
}
