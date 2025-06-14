using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/get")]
    public class GetChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqChallengeEventStageData>();
            var user = GetUser();

            var response = new ResChallengeEventStageData();
            response.RemainTicket = 3;
            response.TeamData = user.UserTeams[1];

            // TODO implement properly
  
            await WriteDataAsync(response);
        }
    }
}
