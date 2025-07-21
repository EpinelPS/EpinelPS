using EpinelPS.Utils;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitle/set")]
    public class SetUserTitleData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetUserTitle req = await ReadData<ReqSetUserTitle>();
            User user = GetUser();
			user.TitleId = req.UserTitleId;
			JsonDb.Save();
            ResSetUserTitle response = new();

            await WriteDataAsync(response);
        }
    }
}
