using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/tactic/get")]
    public class GetTacticAcademyData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetTacticAcademyData req = await ReadData<ReqGetTacticAcademyData>();
            User user = GetUser();

            ResGetTacticAcademyData response = new();
            response.ClearLessons.AddRange(user.CompletedTacticAcademyLessons);

            await WriteDataAsync(response);
        }
    }
}
