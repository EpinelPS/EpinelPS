using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/get")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetAttractiveList req = await ReadData<ReqGetAttractiveList>();
            User user = GetUser();

            ResGetAttractiveList response = new()
            {
                CounselAvailableCount = 3 // TODO
            };

            foreach (NetUserAttractiveData item in user.BondInfo)
            {
                response.Attractives.Add(item);
                item.CanCounselToday = true;
                
            }

            // TODO: ValIdate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}