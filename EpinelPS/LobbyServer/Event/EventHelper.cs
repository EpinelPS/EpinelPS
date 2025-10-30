using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Event
{
    public static class EventHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventHelper));

        public static void AddEvents(ref ResGetEventList response)
        {
            // TODO
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = []; //[.. GameData.Instance.LobbyPrivateBannerTable.Values.Where(b => b.StartDate <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow)];
            Logging.WriteLine($"Found {lobbyPrivateBanners.Count} active lobby private banners.", LogType.Debug);
            log.Debug($"Active lobby private banners: {JsonConvert.SerializeObject(lobbyPrivateBanners)}");

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
                if (events.Count == 0)
                {
                    Logging.WriteLine($"No events found for Banner EventId: {banner.EventId}", LogType.Warning);
                    continue;
                }
                foreach (var ev in events)
                {
                    // Avoid adding duplicate events
                    if (!response.EventList.Any(e => e.Id == ev.Id))
                    {
                        response.EventList.Add(ev);
                    }
                    else
                    {
                        log.Debug($"Skipping duplicate event Id: {ev.Id}");
                    }
                }

                // Additionally, get any gacha events associated with this banner
                List<NetEventData> gachaEvents = GetGachaEventData(banner, eventManagers);
                log.Debug($"Banner EventId: {banner.EventId} has {gachaEvents.Count} associated gacha events: {JsonConvert.SerializeObject(gachaEvents)}");
                if (gachaEvents.Count == 0)
                {
                    Logging.WriteLine($"No gacha events found for Banner EventId: {banner.EventId}", LogType.Warning);
                    continue;
                }
                foreach (var gachaEvent in gachaEvents)
                {
                    // Avoid adding duplicate events
                    if (!response.EventList.Any(e => e.Id == gachaEvent.Id))
                    {
                        response.EventList.Add(gachaEvent);
                    }
                    else
                    {
                        log.Debug($"Skipping duplicate gacha event Id: {gachaEvent.Id}");
                    }
                }
            }

        }

        public static void AddJoinedGachaEvents(ref ResGetJoinedEvent response)
        {
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = [];//[.. GameData.Instance.LobbyPrivateBannerTable.Values.Where(b => b.StartDate <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow)];
            Logging.WriteLine($"Found {lobbyPrivateBanners.Count} active lobby private banners.", LogType.Debug);
            log.Debug($"Active lobby private banners: {JsonConvert.SerializeObject(lobbyPrivateBanners)}");

            if (lobbyPrivateBanners.Count == 0)
            {
                // No active lobby private banners
                Logging.WriteLine("No active lobby private banners found.", LogType.Warning);
                return;
            }

            var eventManagers = GameData.Instance.eventManagers.Values.ToList();
            foreach (var banner in lobbyPrivateBanners)
            {
                List<NetEventData> gachaEvents = GetGachaEventData(banner, eventManagers);
                log.Debug($"Banner EventId: {banner.EventId} has {gachaEvents.Count} associated gacha events: {JsonConvert.SerializeObject(gachaEvents)}");
                if (gachaEvents.Count == 0)
                {
                    Logging.WriteLine($"No gacha events found for Banner EventId: {banner.EventId}", LogType.Warning);
                    continue;
                }
                foreach (var gachaEvent in gachaEvents)
                {
                    // Avoid adding duplicate events
                    if (!response.EventWithJoinData.Any(e => e.EventData.Id == gachaEvent.Id))
                    {
                        response.EventWithJoinData.Add(new NetEventWithJoinData()
                        {
                            EventData = gachaEvent,
                            JoinAt = 0
                        });
                    }
                    else
                    {
                        log.Debug($"Skipping duplicate gacha event Id: {gachaEvent.Id}");
                    }
                }
            }
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
          /*  events.Add(new NetEventData()
            {
                Id = mainEvent.Id,
                EventSystemType = (int)mainEvent.EventSystemType,
                EventStartDate = banner.StartDate.Ticks,
                EventVisibleDate = banner.StartDate.Ticks,
                EventDisableDate = banner.EndDate.Ticks,
                EventEndDate = banner.EndDate.Ticks
            });*/
            // Add child events associated with the main event
            var childEvents = eventManagers.Where(em => em.ParentsEventId == banner.EventId || em.SetField == banner.EventId).ToList();
            foreach (var childEvent in childEvents)
            {
               /* events.Add(new NetEventData()
                {
                    Id = childEvent.Id,
                    EventSystemType = (int)childEvent.EventSystemType,
                    EventStartDate = banner.StartDate.Ticks,
                    EventVisibleDate = banner.StartDate.Ticks,
                    EventDisableDate = banner.EndDate.Ticks,
                    EventEndDate = banner.EndDate.Ticks
                });*/
            }
            return events;
        }

        private static List<NetEventData> GetGachaEventData(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers)
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

            // Find all gacha events (PickupGachaEvent or BoxGachaEvent) that use these banner resource tables
            var gachaEvents = eventManagers.Where(em =>
            eventBannerResourceTables.Contains(em.EventBannerResourceTable)
            && (em.EventSystemType == EventSystemType.PickupGachaEvent || em.EventSystemType == EventSystemType.BoxGachaEvent)).ToList();
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
                    //EventStartDate = banner.StartDate.Ticks,
                    //EventVisibleDate = banner.StartDate.Ticks,
                    ////EventDisableDate = banner.EndDate.Ticks,
                    //EventEndDate = banner.EndDate.Ticks
                });
            }
            return events;
        }

    }
}