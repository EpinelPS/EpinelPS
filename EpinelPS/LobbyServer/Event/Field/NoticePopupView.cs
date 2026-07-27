using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/noticepopup/view")]
public class NoticePopupView : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqViewEventFieldNoticePopup req = await ReadData<ReqViewEventFieldNoticePopup>();
        User user = GetUser();
        ResViewEventFieldNoticePopup response = new();
        //Logging.WriteLine($"{req.EventFieldId},{req.EventFieldNoticePopupTableIds}", LogType.Info);
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
        // TODO
        await WriteDataAsync(response);
    }
}