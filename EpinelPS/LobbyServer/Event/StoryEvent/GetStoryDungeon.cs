using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStoryDungeonEventData req = await ReadData<ReqStoryDungeonEventData>();
            int evid = req.EventId;
            User user = GetUser();


            if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
            {
                eventData = new();
            }

            ResStoryDungeonEventData response = new()
            {
                RemainTicket = 5,

                TeamData = new NetUserTeamData()
                {
                    LastContentsTeamNumber = 1,
                    Type = 20
                }
            };
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });
            // TOOD

            await WriteDataAsync(response);
        }
    }
}
