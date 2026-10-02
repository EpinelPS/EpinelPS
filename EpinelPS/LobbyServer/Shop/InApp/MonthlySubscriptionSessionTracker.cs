using System.Collections.Concurrent;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Shop.InApp;

public static class MonthlySubscriptionSessionTracker
{
    // Tracks when a user claimed a monthly subscription in the current server process session: (UserId, Tid) -> ClaimedAtUtc
    private static readonly ConcurrentDictionary<(ulong UserId, int Tid), DateTime> ClaimedAtUtc = new();

    /// <summary>
    /// Checks whether the user is eligible to claim the daily monthly subscription reward in this session.
    /// Returns true if not yet claimed during this server session, or if claimed before the most recent daily reset.
    /// </summary>
    public static bool CanClaim(User user, int tid)
    {
        if (!ClaimedAtUtc.TryGetValue((user.ID, tid), out var lastClaimUtc))
        {
            return true; // Not claimed in this server session yet!
        }

        // If claimed in this session, but the user had a daily reset since that claim, allow claim!
        if (lastClaimUtc < user.LastReset)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Records that the user has claimed the daily reward for this monthly subscription during this session.
    /// </summary>
    public static void MarkClaimed(User user, int tid)
    {
        ClaimedAtUtc[(user.ID, tid)] = DateTime.UtcNow;
    }
}
