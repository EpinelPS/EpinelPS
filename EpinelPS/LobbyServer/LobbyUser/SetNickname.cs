using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setnickname")]
    public class SetNickname : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetNickname req = await ReadData<ReqSetNickname>();
            User user = GetUser();
            user.Nickname = req.Nickname;

            ResSetNickname response = new()
            {
                Result = SetNicknameResult.Okay,
                Nickname = req.Nickname
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
