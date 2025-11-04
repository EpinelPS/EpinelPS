using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/getnonresettable")]
    public class GetNonResettable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetNonResettableArchiveScenario req = await ReadData<ReqGetNonResettableArchiveScenario>();
            ResGetNonResettableArchiveScenario response = new();

            User user = GetUser();
            foreach (var (evtId, evtData) in user.EventInfo)
            {
                if (evtId == req.EventId)
                {
                    response.ScenarioIdList.AddRange(evtData.CompletedScenarios);
                    break;
                }
            }

            await WriteDataAsync(response);
        }
    }
}
