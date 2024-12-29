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

            var response = new ResSetNicknameFree();
            response.Result = SetNicknameResult.SetNicknameResultOkay;
            response.Nickname = req.Nickname;

            await WriteDataAsync(response);
        }
    }
}
