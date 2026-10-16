using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/noticepopup/view")]
public class NoticePopupView : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqViewArchiveEventFieldNoticePopup req = await ReadData<ReqViewArchiveEventFieldNoticePopup>();
        User user = GetUser();
        ResViewArchiveEventFieldNoticePopup response = new();

        var list = req.EventFieldNoticePopupTableIds.ToList();
        if (user.ViewedNoticePopupTableIds.TryGetValue(req.EventFieldId, out var notice))
        {
            notice.AddRangeUnique(list);
        }
        else
        {
            user.ViewedNoticePopupTableIds.TryAdd(req.EventFieldId, list);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
