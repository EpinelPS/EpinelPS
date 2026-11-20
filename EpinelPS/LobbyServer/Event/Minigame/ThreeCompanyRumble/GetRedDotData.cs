using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.ThreeCompanyRumble;

[GameRequest("/event/minigame/threecompanyrumble/get/reddot/data")]
public class GetRedDotData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetThreeCompanyRumbleRedDotData req = await ReadData<ReqGetThreeCompanyRumbleRedDotData>();

        ResGetThreeCompanyRumbleRedDotData response = new()
        {
            HasEntered = false,
            IsDailyMissionAvailable = false,
            MissionRewardExists = false
        };

        await WriteDataAsync(response);
    }
}
