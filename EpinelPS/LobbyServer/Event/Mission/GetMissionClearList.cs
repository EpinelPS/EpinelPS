using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/getclearlist")]
    public class GetMissionClearList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "eventIdList": [ 60090, 60092, 20001, 20002 ] }
            ReqGetEventMissionClearList req = await ReadData<ReqGetEventMissionClearList>();
            User user = GetUser();
            ResGetEventMissionClearList response = new(); 
            
            try
            {
                response.ResGetEventMissionClearMap.AddRange(EventMissionHelper.GetClearedList(user, req.EventIdList));
            }
            catch (Exception ex)
            {
                Logging.Warn($"GetMissionClearList failed: {ex.Message}");
            }

            await WriteDataAsync(response);
        }
    }
}
