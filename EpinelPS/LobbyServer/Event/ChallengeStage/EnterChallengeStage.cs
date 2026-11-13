using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.ChallengeStage;

[GameRequest("/event/challengestage/enter")]
public class EnterChallengeStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterChallengeEventStage req = await ReadData<ReqEnterChallengeEventStage>();
        User user = GetUser();
        ResEnterChallengeEventStage response = new();

        await WriteDataAsync(response);
    }
}
