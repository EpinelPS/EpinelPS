using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/mail/get")]
public class GetMail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMailData req = await ReadData<ReqGetMailData>();
        User user = GetUser();
        ResGetMailData response = new();
        if (user.MailDatas.Count > 0)
        {
            foreach (var item in user.MailDatas.Values)
            {
                response.Mail.Add(item);
            }
        }
        await WriteDataAsync(response);
    }
}