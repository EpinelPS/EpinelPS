using EpinelPS.Net;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/tactic/get")]
    public class GetTacticAcademyData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<GetTacticAcademyDataRequest>();
            var user = GetUser();

            var response = new GetTacticAcademyDataResponse();
            response.CompletedLessons.AddRange(user.CompletedTacticAcademyLessons);

            await WriteDataAsync(response);
        }
    }
}
