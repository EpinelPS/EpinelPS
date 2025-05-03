using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/SetNickNameInTutorial")]
    public class SetNicknameInTutorial : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetNicknameInTutorial>();
            var user = GetUser();
            user.Nickname = req.Nickname;

            var response = new ResSetNicknameInTutorial
            {
                Result = SetNicknameResult.SetNicknameResultOkay,
                Nickname = req.Nickname
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
