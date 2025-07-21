using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Team
{
    [PacketPath("/team/get")]
    public class GetTeamData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetTeamData req = await ReadData<ReqGetTeamData>();
            User user = GetUser();

            ResGetTeamData response = new();

            // NOTE: Keep this in sync with EnterLobbyServer code
            if (user.Characters.Count > 0)
            {
                foreach (KeyValuePair<int, NetUserTeamData> item in user.UserTeams)
                {
                    response.TypeTeams.Add(item.Value);
                }
            }
            await WriteDataAsync(response);
        }
    }
}
