using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/scenario/get")]
public class ScenarioGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetNKSV2Scenario req = await ReadData<ReqGetNKSV2Scenario>();
        User user = GetUser();
        ResGetNKSV2Scenario response = new();

        //Logging.WriteLine($"{req.NKsId}", LogType.Info);

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            response.ScenarioIdList.AddRange(nksv2Data.CompletedScenarios);
        }
        else
        {
            Nksv2Data nksv2DataNew = new();
            List<EventNKSMissionRecord>? missionlist = GameData.Instance.EventNKSMissionTable.Values
                .Where(x => x.CommonSettingsID == req.NKsId).ToList();
            if (missionlist.Count > 0)
            {
                foreach (var item in missionlist)
                {
                    MiniGameNKSV2MissionProgress missionProgress = new()
                    {
                        NKsMissionId = item.Id,
                        CreatedAt = DateTime.UtcNow.ToTimestamp(),
                        Progress = 0,
                        Seq = (long)User.GenerateMsn()
                    };

                    nksv2DataNew.MissionProgressData.TryAdd(item.Id, missionProgress);
                }

            }

            nksv2DataNew.ProgressJson = "";
            user.Nksv2Datas.TryAdd(req.NKsId, nksv2DataNew);
        }

        // TODO
        await WriteDataAsync(response);
    }
}