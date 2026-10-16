using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/delete")]
public class DeleteBadge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqDeleteBadge req = await ReadData<ReqDeleteBadge>();
        User user = GetUser();

        ResDeleteBadge response = new();

        bool hasUnclaimedMail = user.MailDatas.Values.Any(m => m.State == 1 && m.HasReward);

        foreach (long badgeId in req.BadgeSeqList)
        {
            // Protect Mailbox badge: If the player still has unclaimed reward mail, do NOT delete the Mailbox badge!
            // This prevents client automatic badge acknowledgment from extinguishing the red dot
            // while rewards are still uncollected.
            user.Badges.RemoveAll(x => x.Seq == badgeId && (x.BadgeContent != BadgeContents.Mailbox || !hasUnclaimedMail));
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
