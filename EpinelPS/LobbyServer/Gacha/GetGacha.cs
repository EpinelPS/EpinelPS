using EpinelPS.Data;
using EpinelPS.LobbyServer.Event;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/Gacha/Get")]
public class GetGacha : LobbyMessage
{
    private static readonly GachaPremiumType[] AlwaysVisibleTypes =
    [
        GachaPremiumType.GachaPremium,
        GachaPremiumType.GachaFriend
    ];

    private static readonly GachaPremiumType[] EventVisibleTypes =
    [
        GachaPremiumType.GachaPickup,
        GachaPremiumType.GachaSelectup
    ];

    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetGachaData>();
        User user = GetUser();

        ResGetGachaData response = new();
        HashSet<int> addedGachaTypes = [];

        IEnumerable<GachaTypeRecord> records = GetVisibleGachaTypes(user)
            .OrderBy(gacha => gacha.OrderId)
            .ThenBy(gacha => gacha.Id);

        foreach (GachaTypeRecord record in records)
        {
            AddGacha(response, addedGachaTypes, record, user);
        }

        await WriteDataAsync(response);
    }

    private static IEnumerable<GachaTypeRecord> GetVisibleGachaTypes(User user)
    {
        foreach (GachaPremiumType type in AlwaysVisibleTypes)
        {
            GachaTypeRecord? record = GameData.Instance.gachaTypes.Values
                .Where(gacha => gacha.Type == type)
                .OrderBy(gacha => gacha.OrderId)
                .FirstOrDefault();

            if (record != null)
            {
                yield return record;
            }
        }

        if (GameData.Instance.gachaTypes.TryGetValue(3, out GachaTypeRecord? tutorial))
        {
            yield return tutorial;
        }

        HashSet<int> activeEventIds = GetActiveEventIds(user);
        List<GachaTypeRecord> activeEventGachas = [.. GameData.Instance.gachaTypes.Values
            .Where(gacha => EventVisibleTypes.Contains(gacha.Type))
            .Where(gacha => gacha.EventId != 0 && activeEventIds.Contains(gacha.EventId))];

        if (activeEventGachas.Count == 0)
        {
            activeEventGachas = [.. EventVisibleTypes
                .Select(type => GameData.Instance.gachaTypes.Values
                    .Where(gacha => gacha.Type == type && gacha.EventId != 0)
                    .OrderBy(gacha => gacha.OrderId)
                    .FirstOrDefault())
                .Where(gacha => gacha != null)!];
        }

        foreach (GachaTypeRecord record in activeEventGachas)
        {
            yield return record;
        }
    }

    private static HashSet<int> GetActiveEventIds(User user)
    {
        HashSet<int> activeEventIds = [];
        List<EventManagerRecord> eventManagers = [.. GameData.Instance.eventManagers.Values];

        foreach (LobbyPrivateBannerRecord banner in EventHelper.GetLobbyPrivateBannerData(user))
        {
            activeEventIds.Add(banner.EventId);

            List<EventManagerRecord> relatedEvents = [.. eventManagers
                .Where(em => em.ParentsEventId == banner.EventId || em.SetField == banner.EventId)];

            foreach (EventManagerRecord relatedEvent in relatedEvents)
            {
                activeEventIds.Add(relatedEvent.Id);
            }

            HashSet<string> eventBannerResourceTables = [.. relatedEvents
                .Where(em => !string.IsNullOrEmpty(em.EventBannerResourceTable))
                .Where(em => em.EventBannerResourceTable!.StartsWith("event_", StringComparison.Ordinal))
                .Select(em => em.EventBannerResourceTable!)];

            foreach (EventManagerRecord gachaEvent in eventManagers
                .Where(em => !string.IsNullOrEmpty(em.EventBannerResourceTable))
                .Where(em => eventBannerResourceTables.Contains(em.EventBannerResourceTable!))
                .Where(em => em.EventSystemType == EventSystemType.PickupGachaEvent))
            {
                activeEventIds.Add(gachaEvent.Id);
            }
        }

        return activeEventIds;
    }

    private static void AddGacha(ResGetGachaData response, HashSet<int> addedGachaTypes, GachaTypeRecord record, User user)
    {
        if (!addedGachaTypes.Add(record.Id))
        {
            return;
        }

        response.Gacha.Add(new NetUserGachaData()
        {
            GachaType = record.Id,
            PlayCount = record.Type == GachaPremiumType.GachaTutorial ? user.GachaTutorialPlayCount : 0
        });

        if (record.DailyFreeGachaEventId != 0)
        {
            response.GachaEventData.Add(new NetGachaEvent()
            {
                GachaTypeId = record.Id,
                FreeCount = 1
            });
        }

        if (record.Type == GachaPremiumType.GachaPickup)
        {
            int pickupCharacterId = GetPickupCharacterId(record);
            if (pickupCharacterId != 0)
            {
                response.MultipleCustom.Add(new NetGachaCustomData()
                {
                    Type = (int)record.Type,
                    Tid = pickupCharacterId
                });
            }
        }

        if (record.Type == GachaPremiumType.GachaSelectup)
        {
            GachaSelectupListRecord_Raw? defaultSelectup = GachaPoolResolver.GetSelectedOrDefaultSelectup(user, record.Id);

            if (defaultSelectup != null)
            {
                response.GachaSelectupData.Add(new NetUserGachaSelectupData()
                {
                    GachaTypeId = record.Id,
                    GachaSelectupId = defaultSelectup.Id
                });
            }
        }
    }

    private static int GetPickupCharacterId(GachaTypeRecord record)
    {
        if (record.PickupCharGroupId == 0)
        {
            return 0;
        }

        return GameData.Instance.GachaListProb.Values
            .Where(prob => prob.GroupId == record.PickupCharGroupId)
            .Where(prob => prob.GachaSubType == GachaSubType.PickupCharacter)
            .OrderByDescending(prob => prob.Prob)
            .ThenBy(prob => prob.Id)
            .Select(prob => prob.GachaId)
            .FirstOrDefault();
    }
}
