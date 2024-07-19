using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/get")]
    public class GetTeamData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetTeamData>();
            var user = GetUser();

            var response = new ResGetTeamData();

            // NOTE: Keep this in sync with EnterLobbyServer code
            if (user.Characters.Count > 0)
            {
                foreach (var item in user.UserTeams)
                {
                    response.TypeTeams.Add(item.Value);
                }
            }
            await WriteDataAsync(response);
        }
    }
}
