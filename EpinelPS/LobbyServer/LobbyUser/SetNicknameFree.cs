using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setnicknamefree")]
    public class SetNicknameFree : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetNicknameFree req = await ReadData<ReqSetNicknameFree>();
            User user = GetUser();
            user.Nickname = req.Nickname;

            ResSetNicknameFree response = new()
            {
                Result = SetNicknameResult.Okay,
                Nickname = req.Nickname
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
