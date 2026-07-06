using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Shop/Enter")]
public class ShopEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterMiniGameTtsPackageShop req = await ReadData<ReqEnterMiniGameTtsPackageShop>();
        User user = GetUser();
        ResEnterMiniGameTtsPackageShop response = new();

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            List<EventTTSMissionRecord_Raw>? shopmission = GameData.Instance.EventTTSMissionTable.Values
            .Where(s => s.MissionType == EventTTSMissionType.EnterTTSPackageShop)
            .ToList();

            if (shopmission.Count >0)
            {
                foreach (var item in shopmission)
                {
                    if (ttsData.MissionData.TryGetValue(item.Id, out var missionData))
                    {
                        missionData.Progress += 1;
                    }
                }
            } 

        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}