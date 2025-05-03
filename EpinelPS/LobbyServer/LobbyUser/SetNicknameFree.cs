using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setnicknamefree")]
    public class SetNicknameFree : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetNicknameFree>();
            var user = GetUser();
            user.Nickname = req.Nickname;

            var response = new ResSetNicknameFree
            {
                Result = SetNicknameResult.SetNicknameResultOkay,
                Nickname = req.Nickname
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
