using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/SetNickNameInTutorial")]
    public class SetNicknameInTutorial : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetNicknameInTutorial req = await ReadData<ReqSetNicknameInTutorial>();
            User user = GetUser();
            user.Nickname = req.Nickname;

            ResSetNicknameInTutorial response = new()
            {
                Result = SetNicknameResult.Okay,
                Nickname = req.Nickname
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
