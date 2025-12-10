using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStoryDungeonEventData req = await ReadData<ReqStoryDungeonEventData>();
            User user = GetUser();

            // Get user event data, if not exist, create new one
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new() { LastDay = user.GetDateDay(), FreeTicket = 5};
                user.EventInfo.TryAdd(req.EventId, eventData);
            }
            

            ResStoryDungeonEventData response = new()
            {
                RemainTicket = EventStoryHelper.GetTicket(user, req.EventId),
                TeamData = new NetUserTeamData
                { 
                    Type = (int)TeamType.StoryEvent
                },
            };

            if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
            {
                response.TeamData = teamData;
            }
            foreach (var stageId in eventData.ClearedStages)
            {
                response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
                {
                    StageId = stageId
                });
            }
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
            {
                StageId = eventData.LastStage
            });
            // TODO

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
