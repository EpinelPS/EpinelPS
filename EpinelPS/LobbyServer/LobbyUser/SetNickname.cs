using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setnickname")]
    public class SetNickname : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetNickname>();
            var user = GetUser();
            user.Nickname = req.Nickname;

            var response = new ResSetNickname();
            response.Result = SetNicknameResult.SetNicknameResultOkay;
            response.Nickname = req.Nickname;

            await WriteDataAsync(response);
        }
    }
}
