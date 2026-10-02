using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/sync")]
public class SyncBadge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSyncBadge req = await ReadData<ReqSyncBadge>();
        User user = GetUser();

        ResSyncBadge response = new();

        long maxClientSeq = Math.Max(req.LastBadgeSeq, req.LastUniqueBadgeSeq);
        if (maxClientSeq > user.LastBadgeSeq)
        {
            user.LastBadgeSeq = maxClientSeq;
        }

        // Clean up any legacy MailboxMessage badges that cause client-side SyncedBadgeHandler / ViewMail crash
        user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.MailboxMessage);

        bool hasUnclaimedMail = user.MailDatas.Values.Any(m => m.State == 1 && m.HasReward);
        if (hasUnclaimedMail)
        {
            // CRITICAL: The lobby Mailbox header icon in the client is bound to UniqueBadgeList.
            // Furthermore, the client only illuminates the red dot if the badge sequence
            // is strictly greater than the client's acknowledged sequence (maxClientSeq).
            long freshSeq = Math.Max(user.LastBadgeSeq, maxClientSeq) + 1;
            user.LastBadgeSeq = freshSeq;

            user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.Mailbox);
            user.Badges.Add(new BadgeModel
            {
                BadgeContent = BadgeContents.Mailbox,
                BadgeGuid = Guid.NewGuid().ToString(),
                Location = string.Empty,
                Seq = freshSeq
            });
        }
        else
        {
            // No unclaimed rewards remain -> extinguish the Mailbox red dot badge
            user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.Mailbox || b.BadgeContent == BadgeContents.MailboxMessage);
        }

        foreach (BadgeModel item in user.Badges)
        {
            response.BadgeList.Add(item.ToNet());
            response.UniqueBadgeList.Add(item.ToUniqueNet());
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
