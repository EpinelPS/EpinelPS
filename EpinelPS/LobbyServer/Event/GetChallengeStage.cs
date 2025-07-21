using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/get")]
    public class GetChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqChallengeEventStageData req = await ReadData<ReqChallengeEventStageData>();
            Database.User user = GetUser();

            ResChallengeEventStageData response = new()
            {
                RemainTicket = 3,
                TeamData = user.UserTeams[1]
            };

            // TODO implement properly

            await WriteDataAsync(response);
        }
    }
}
