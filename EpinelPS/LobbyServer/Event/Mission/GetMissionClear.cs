using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/getclear")]
    public class GetMissionClear : LobbyMsgHandler
    {

        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClear>(); //EventId
            User user = GetUser();

            ResGetEventMissionClear response = new();

            try
            {
                response.EventMissionClearList.AddRange(EventMissionHelper.GetCleared(user, req.EventId));
            }
            catch (Exception ex)
            {
                Logging.Warn($"GetMissionClear failed: {ex.Message}");
            }

            // TODO
            await WriteDataAsync(response);
        }
    }
}
