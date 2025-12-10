using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    public static class EventStoryHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventStoryHelper));

        /// <summary>
        /// Get user remain ticket by event id
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public static int GetTicket(User user, int eventId)
        {

            int freeTicket = 5; // Default ticket is 5

            // Get user event data, if exists, get free ticket
            if (user.EventInfo.TryGetValue(eventId, out var eventData))
            {
                freeTicket = eventData.FreeTicket;
            }

            // Get item ticket information and free ticket max
            (ItemData itemTicket, int freeTicketMax) = GetItemTicket(user, eventId);

            // Get remain item ticket
            int remainItemTicket = itemTicket?.Count ?? 0;

            // If free ticket is greater than free ticket max, set free ticket to free ticket max
            if (freeTicket > freeTicketMax) freeTicket = freeTicketMax;

            int dateDay = user.GetDateDay();
            // If dateDay is greater than last day, update user free ticket and last day
            if (dateDay > eventData.LastDay)
            {
                Logging.WriteLine($"GetTicket ResetFreeTicket DateDay: {dateDay}, LastDay: {eventData.LastDay}, FreeTicketMax: {freeTicketMax}", LogType.Debug);
                freeTicket = freeTicketMax;
                user.EventInfo[eventId].FreeTicket = freeTicket;
                user.EventInfo[eventId].LastDay = dateDay;
            }

            // Remain ticket is free ticket + item ticket
            int remainTicket = freeTicket + remainItemTicket;

            Logging.WriteLine($"GetTicket FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}", LogType.Debug);
            return remainTicket;
        }

        /// <summary>
        /// Subtract user remaining ticket by event id and value
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventId"></param>
        /// <param name="val"></param>
        /// <returns>remaining ticket</returns>
        public static int SubtractTicket(User user, int eventId, int val)
        {
            int freeTicket = 5; // Default ticket is 5

            // Get user event data, if exists, get free ticket
            if (user.EventInfo.TryGetValue(eventId, out var eventData))
            {
                freeTicket = eventData.FreeTicket;
            }

            // Get item ticket information
            (ItemData itemTicket, _) = GetItemTicket(user, eventId);
            // Get remain item ticket 
            int remainItemTicket = itemTicket?.Count ?? 0;

            // If free ticket is enough to subtract
            if (freeTicket >= val)
            {
                freeTicket -= val;
                user.EventInfo[eventId].FreeTicket = freeTicket;

                int remainTicket = freeTicket + remainItemTicket;
                Logging.WriteLine($"SubtractTicket Value: {val}, FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}", LogType.Debug);
                return remainTicket;
            }
            else
            {
                // If free ticket is not enough to subtract, subtract free ticket and subtract item ticket
                int SubtractItemTicket = val - freeTicket;
                user.EventInfo[eventId].FreeTicket = 0;
                if (itemTicket is not null)
                {
                    user.RemoveItemBySerialNumber(itemTicket.Isn, SubtractItemTicket);
                }

                freeTicket = 0;
                // Remain ticket is free ticket + item ticket
                int remainTicket = freeTicket + remainItemTicket;
                Logging.WriteLine($"SubtractTicket Value: {val}, FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}", LogType.Debug);
                return remainTicket;
            }

        }

        /// <summary>
        /// Get user item ticket and free ticket max by event id
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        private static (ItemData itemTicket, int freeTicketMax) GetItemTicket(User user, int eventId)
        {
            int freeTicketMax = 5; // Default free ticket max is 5
            // Get event story data
            var eventStory = GameData.Instance.EventStoryTable.Values.FirstOrDefault(x => x.EventId == eventId);

            // If event story data is null or auto charge id is 0, return null and default free ticket max
            if (eventStory is null || eventStory.AutoChargeId == 0) return (null, freeTicketMax);
            log.Debug($"GetItemTicket EventId: {eventId}, EventStory: {JsonConvert.SerializeObject(eventStory)}");

            // If auto charge data is null, return null and default free ticket max
            if (!GameData.Instance.AutoChargeTable.TryGetValue(eventStory.AutoChargeId, out var autoCharge)) return (null, 5);
            log.Debug($"GetItemTicket AutoChargeId: {eventStory.AutoChargeId}, AutoCharge: {JsonConvert.SerializeObject(autoCharge)}");

            // If auto charge max is 0, return null and default free ticket max
            if (autoCharge.AutoChargeMax == 0) return (null, freeTicketMax);

            freeTicketMax = autoCharge.AutoChargeMax; // Set free ticket max to auto charge max

            // Get user item
            var userItem = user.Items.FirstOrDefault(x => x.ItemType == autoCharge.ItemId);
            log.Debug($"GetItemTicket UserItem: {(userItem is not null ? JsonConvert.SerializeObject(userItem) : null)}");
            return (userItem, freeTicketMax); // Return user item and free ticket max
        }


    }
}