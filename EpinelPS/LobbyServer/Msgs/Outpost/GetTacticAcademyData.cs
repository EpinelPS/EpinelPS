using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/tactic/get")]
    public class GetTacticAcademyData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetTacticAcademyData>();
            var user = GetUser();

            var response = new ResGetTacticAcademyData();
            response.ClearLessons.AddRange(user.CompletedTacticAcademyLessons);

            await WriteDataAsync(response);
        }
    }
}
