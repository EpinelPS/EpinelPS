using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Event
{
    public class EventHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventHelper));

        public static void AddEvents(User user, ref ResGetEventList response)
        {
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = GetLobbyPrivateBannerData(user);
            if (lobbyPrivateBanners.Count == 0)
            {
                // No active lobby private banners
                Logging.WriteLine("No active lobby private banners found.", LogType.Warning);
                return;
            }

            var eventManagers = GameData.Instance.eventManagers.Values.ToList();
            foreach (var banner in lobbyPrivateBanners)
            {
                // Get all events (including child events) associated with this banner
                List<NetEventData> events = GetEventData(banner, eventManagers);
                log.Debug($"Banner EventId: {banner.EventId} has {events.Count} associated events: {JsonConvert.SerializeObject(events)}");
                AddEvents(ref response, events);

                // Additionally, get any gacha events associated with this banner
                List<EventSystemType> systemTypes = [EventSystemType.PickupGachaEvent, EventSystemType.BoxGachaEvent, EventSystemType.LoginEvent];
                List<NetEventData> gachaEvents = GetEventDataBySystemTypes(banner, eventManagers, systemTypes);
                log.Debug($"Banner EventId: {banner.EventId} has {gachaEvents.Count} associated gacha events: {JsonConvert.SerializeObject(gachaEvents)}");
                AddEvents(ref response, gachaEvents);

                // add challenge events
                var challengeEvents = GetChallengeEventData(banner, eventManagers);
                log.Debug($"Banner EventId: {banner.EventId} has {challengeEvents.Count} associated challenge events: {JsonConvert.SerializeObject(challengeEvents)}");
                AddEvents(ref response, challengeEvents);
            }
            // add daily mission events
            List<NetEventData> dailyMissionEvents = GetDailyMissionEventData(eventManagers);
            log.Debug($"Found {dailyMissionEvents.Count} associated daily mission events: {JsonConvert.SerializeObject(dailyMissionEvents)}");
            AddEvents(ref response, dailyMissionEvents);
        }

        public static void AddJoinedEvents(User user, ref ResGetJoinedEvent response)
        {
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = GetLobbyPrivateBannerData(user);
            if (lobbyPrivateBanners.Count == 0)
            {
                // No active lobby private banners
                Logging.WriteLine("No active lobby private banners found.", LogType.Warning);
                return;
            }

            var eventManagers = GameData.Instance.eventManagers.Values.ToList();
            foreach (var banner in lobbyPrivateBanners)
            {
                // add gacha events
                List<EventSystemType> systemTypes = [EventSystemType.PickupGachaEvent, EventSystemType.BoxGachaEvent, EventSystemType.LoginEvent];
                List<NetEventData> gachaEvents = GetEventDataBySystemTypes(banner, eventManagers, systemTypes);
                log.Debug($"Banner EventId: {banner.EventId} has {gachaEvents.Count} associated gacha events: {JsonConvert.SerializeObject(gachaEvents)}");
                AddJoinedEvents(ref response, gachaEvents);

                // add challenge events
                List<NetEventData> challengeEvents = GetChallengeEventData(banner, eventManagers);
                log.Debug($"Banner EventId: {banner.EventId} has {challengeEvents.Count} associated challenge events: {JsonConvert.SerializeObject(challengeEvents)}");
                AddJoinedEvents(ref response, challengeEvents);
            }
            // add daily mission events
            List<NetEventData> dailyMissionEvents = GetDailyMissionEventData(eventManagers);
            log.Debug($"Found {dailyMissionEvents.Count} associated daily mission events: {JsonConvert.SerializeObject(dailyMissionEvents)}");
            AddJoinedEvents(ref response, dailyMissionEvents);
        }

        private static List<NetEventData> GetEventData(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];

            if (!eventManagers.Any(em => em.Id == banner.EventId))
            {
                Logging.WriteLine($"No event manager found for Banner EventId: {banner.EventId}", LogType.Warning);
                return events;
            }
            // Add the main event associated with the banner
            var mainEvent = eventManagers.First(em => em.Id == banner.EventId);
            events.Add(new NetEventData()
            {
                Id = mainEvent.Id,
                EventSystemType = (int)mainEvent.EventSystemType,
                // EventStartDate = banner.StartDate.Ticks,
                // EventVisibleDate = banner.StartDate.Ticks,
                // EventDisableDate = banner.EndDate.Ticks,
                // EventEndDate = banner.EndDate.Ticks
            });
            // Add child events associated with the main event
            var childEvents = eventManagers.Where(em => em.ParentsEventId == banner.EventId || em.SetField == banner.EventId).ToList();
            foreach (var childEvent in childEvents)
            {
                events.Add(new NetEventData()
                {
                    Id = childEvent.Id,
                    EventSystemType = (int)childEvent.EventSystemType,
                    // EventStartDate = banner.StartDate.Ticks,
                    // EventVisibleDate = banner.StartDate.Ticks,
                    // EventDisableDate = banner.EndDate.Ticks,
                    // EventEndDate = banner.EndDate.Ticks
                });
            }
            return events;
        }

        private static List<NetEventData> GetEventDataBySystemTypes(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers, List<EventSystemType> systemTypes)
        {
            List<NetEventData> events = [];
            // Find all event banner resource tables associated with this banner's EventId
            List<string> eventBannerResourceTables = [.. eventManagers.Where(em =>
                (em.SetField == banner.EventId || em.ParentsEventId == banner.EventId)
                && em.EventBannerResourceTable.StartsWith("event_")).Select(em => em.EventBannerResourceTable)];
            eventBannerResourceTables = [.. eventBannerResourceTables.Distinct()];
            log.Debug($"Banner EventId: {banner.EventId} has {eventBannerResourceTables.Count} associated event banner resource tables: {JsonConvert.SerializeObject(eventBannerResourceTables)}");
            if (eventBannerResourceTables.Count == 0)
            {
                Logging.WriteLine($"No associated event banner resource tables found for Banner EventId: {banner.EventId}", LogType.Warning);
                return events;
            }

            // Find all events matching the banner resource tables and specified system types
            var gachaEvents = eventManagers.Where(em =>
            eventBannerResourceTables.Contains(em.EventBannerResourceTable)
            && systemTypes.Contains(em.EventSystemType)).ToList();
            log.Debug($"Found {gachaEvents.Count} gacha events from banner resource tables: {JsonConvert.SerializeObject(gachaEvents)}");
            if (gachaEvents.Count == 0)
            {
                Logging.WriteLine($"No gacha events found for Banner EventId: {banner.EventId}", LogType.Warning);
                return events;
            }

            // Add each gacha event to the list
            foreach (var gachaEvent in gachaEvents)
            {
                events.Add(new NetEventData()
                {
                    Id = gachaEvent.Id,
                    EventSystemType = (int)gachaEvent.EventSystemType,
                    // EventStartDate = banner.StartDate.Ticks,
                    // EventVisibleDate = banner.StartDate.Ticks,
                    // EventDisableDate = banner.EndDate.Ticks,
                    // EventEndDate = banner.EndDate.Ticks
                });
            }
            return events;
        }

        private static List<NetEventData> GetChallengeEventData(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];

            // Find all challenge events (ChallengeModeEvent) associated with this banner's EventId
            var challengeEvents = eventManagers.Where(em =>
            em.ParentsEventId == banner.EventId && em.EventSystemType == EventSystemType.ChallengeModeEvent).ToList();
            log.Debug($"Found {challengeEvents.Count} challenge events from banner resource tables: {JsonConvert.SerializeObject(challengeEvents)}");
            if (challengeEvents.Count == 0)
            {
                Logging.WriteLine($"No challenge events found for Banner EventId: {banner.EventId}", LogType.Warning);
                return events;
            }

            // Add each challenge event to the list
            foreach (var challengeEvent in challengeEvents)
            {
                events.Add(new NetEventData()
                {
                    Id = challengeEvent.Id,
                    EventSystemType = (int)challengeEvent.EventSystemType,
                    // EventStartDate = banner.StartDate.Ticks,
                    // EventVisibleDate = banner.StartDate.Ticks,
                    // EventDisableDate = banner.EndDate.Ticks,
                    // EventEndDate = banner.EndDate.Ticks
                });
            }
            return events;
        }

        /// <summary>
        /// Get active lobby private banner data
        /// </summary>
        /// <returns>List of active lobby private banners</returns>
        public static List<LobbyPrivateBannerRecord> GetLobbyPrivateBannerData(User user)
        {
            var lobbyPrivateBannerIds = user.LobbyPrivateBannerIds;
            var lobbyPrivateBannerRecords = GameData.Instance.LobbyPrivateBannerTable.Values;
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = [];
            if (lobbyPrivateBannerIds is not null && lobbyPrivateBannerIds.Count > 0)
            {
                lobbyPrivateBanners = [.. lobbyPrivateBannerRecords.Where(b => lobbyPrivateBannerIds.Contains(b.Id))];
            }
            else
            {
                lobbyPrivateBanners.Add(lobbyPrivateBannerRecords.OrderBy(b => b.Id).Last());
            }
            Logging.WriteLine($"Found {lobbyPrivateBanners.Count} active lobby private banners.", LogType.Debug);
            log.Debug($"Active lobby private banners: {JsonConvert.SerializeObject(lobbyPrivateBanners)}");
            return lobbyPrivateBanners;
        }

        private static void AddEvents(ref ResGetEventList response, List<NetEventData> eventDatas)
        {
            foreach (var eventData in eventDatas)
            {
                // if (eventData.Id == 70115) continue;
                // Avoid adding duplicate events
                if (!response.EventList.Any(e => e.Id == eventData.Id))
                {
                    if (eventData.EventStartDate == 0) eventData.EventStartDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventVisibleDate == 0) eventData.EventVisibleDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventDisableDate == 0) eventData.EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks;
                    if (eventData.EventEndDate == 0) eventData.EventEndDate = DateTime.UtcNow.AddDays(30).Ticks;
                    response.EventList.Add(eventData);
                }
                else
                {
                    log.Debug($"Skipping duplicate event Id: {eventData.Id}");
                }
            }
        }

        private static void AddJoinedEvents(ref ResGetJoinedEvent response, List<NetEventData> eventDatas)
        {
            foreach (var eventData in eventDatas)
            {
                if (eventData.Id == 70115) continue;
                // Avoid adding duplicate events
                if (!response.EventWithJoinData.Any(e => e.EventData.Id == eventData.Id))
                {
                    if (eventData.EventStartDate == 0) eventData.EventStartDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventVisibleDate == 0) eventData.EventVisibleDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventDisableDate == 0) eventData.EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks;
                    if (eventData.EventEndDate == 0) eventData.EventEndDate = DateTime.UtcNow.AddDays(30).Ticks;
                    response.EventWithJoinData.Add(new NetEventWithJoinData()
                    {
                        
                        EventData = eventData,
                        JoinAt = 0
                    });
                }
                else
                {
                    log.Debug($"Skipping duplicate event Id: {eventData.Id}");
                }
            }
        }

        private static List<NetEventData> GetDailyMissionEventData(List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];

            var dailyEventIds = GameData.Instance.DailyMissionEventSettingTable.Values.Select(de => de.EventId).ToList();
            log.Debug($"Daily Mission Event IDs: {JsonConvert.SerializeObject(dailyEventIds)}");
            var dailyEvents = eventManagers.Where(em => dailyEventIds.Contains(em.Id)).ToList();
            log.Debug($"Found {dailyEvents.Count} daily events: {JsonConvert.SerializeObject(dailyEvents)}");
            if (dailyEvents.Count == 0)
            {
                Logging.WriteLine("No daily events found.", LogType.Warning);
                return events;
            }

            // Add each daily event to the list
            foreach (var dailyEvent in dailyEvents)
            {
                events.Add(new NetEventData()
                {
                    Id = dailyEvent.Id,
                    EventSystemType = (int)dailyEvent.EventSystemType,
                    EventStartDate = DateTime.UtcNow.Ticks,
                    EventVisibleDate = DateTime.UtcNow.Ticks,
                    EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks,
                    EventEndDate = DateTime.UtcNow.AddDays(30).Ticks
                });
            }
            return events;
        }

    }
}