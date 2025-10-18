using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/get")]
    public class GetChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqChallengeEventStageData req = await ReadData<ReqChallengeEventStageData>();
            User user = GetUser();

            ResChallengeEventStageData response = new()
            {
                RemainTicket = 3,
                TeamData = new NetUserTeamData
                {
                    Type = (int)TeamType.StoryEvent
                },
            };
            // check if user has a team for this type
            if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
            {
                response.TeamData = teamData;
            }
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new() { LastStage = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }
            
            // placeholder response data for last cleared stage
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
