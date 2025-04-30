using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/get")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAttractiveList>();
            var user = GetUser();

            ResGetAttractiveList response = new();
            response.CounselAvailableCount = 3; // TODO

            foreach(var item in user.BondInfo)
            {
                response.Attractives.Add(item);
                item.CanCounselToday = true;
                item.Exp = 9999; // TODO
                item.Level = 10;
            }


            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
