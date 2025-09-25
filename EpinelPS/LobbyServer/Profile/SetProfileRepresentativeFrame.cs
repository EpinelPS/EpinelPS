using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Profile
{
    [PacketPath("/user/setprofilerepresentativeframe")]
    public class SetProfileRepresentativeFrame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetProfileRepresentativeFrame req = await ReadData<ReqSetProfileRepresentativeFrame>();
            User user = GetUser();
            ResSetProfileRepresentativeFrame response = new();
            user.ProfileRepresentativeFrame = req;
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}