using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqStoryDungeonEventData>();
            var evid = req.EventId;
			var user = GetUser();


            if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
            {
                eventData = new();
            }

            var response = new ResStoryDungeonEventData();
            response.RemainTicket = 5;

			response.TeamData = new NetUserTeamData()
			{
				LastContentsTeamNumber = 1,
				Type = 20
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
