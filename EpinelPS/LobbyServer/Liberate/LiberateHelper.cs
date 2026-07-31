using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Liberate;

public class LiberateHelper
{
    public static List<NetLiberateMissionData> GetMissions(int characterId, int missgroup, int randomSubGroupId)
    {
        List<LiberateMissionRecord>? missions = GameData.Instance.LiberateMissionTable.Values
        .Where(x => x.GroupId == missgroup && x.SubGroupId == randomSubGroupId)
        .ToList();

        List<NetLiberateMissionData> netLiberates = new();

        if (missions.Count > 0)
        {
            int id = 1;
            foreach (var item in missions)
            {
                netLiberates.Add(new NetLiberateMissionData()
                {
                    LiberateCharacterId = characterId,
                    Id = id,
                    MissionState = LiberateMissionState.Running,
                    MissionTid = item.Id,
                    CreatedAt = DateTime.UtcNow.Ticks,
                    ReceivedAt = DateTime.UtcNow.Ticks,
                    TriggerStartAt = DateTime.UtcNow.Ticks,
                    TriggerEndAt = DateTime.UtcNow.AddHours(24).Ticks,
                });
                id++;
            }

        }

        return netLiberates;
    }




}
