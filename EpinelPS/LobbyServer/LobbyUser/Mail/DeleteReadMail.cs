using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser.Mail;

[GameRequest("/mail/deletereadmail")]
public class DeleteReadMail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqDeleteReadMail req = await ReadData<ReqDeleteReadMail>();
        User user = GetUser();
        ResDeleteReadMail response = new();
        List<long>? keysToRemove =user.MailDatas.Where(x => x.Value.State == 2).Select(x => x.Key).ToList();
        foreach (var key in keysToRemove)
        {
            user.MailDatas.Remove(key);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}