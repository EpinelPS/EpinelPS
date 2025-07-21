using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/get")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetAttractiveList req = await ReadData<ReqGetAttractiveList>();
            Database.User user = GetUser();

            ResGetAttractiveList response = new()
            {
                CounselAvailableCount = 3 // TODO
            };

            foreach (NetUserAttractiveData item in user.BondInfo)
            {
                response.Attractives.Add(item);
                item.CanCounselToday = true;
                item.Exp = 9999; // TODO
                item.Lv = 10;
            }


            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
