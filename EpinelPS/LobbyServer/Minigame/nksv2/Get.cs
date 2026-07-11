using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameNKSV2Data req = await ReadData<ReqGetMiniGameNKSV2Data>();
        User user = GetUser();
        ResGetMiniGameNKSV2Data response = new();

        //Logging.WriteLine($"{req.NKsId}", LogType.Info);

        if (user.Nksv2Datas.TryGetValue(req.NKsId,out var nksv2Data))
        {
            var missprodata = MiniGameHelper
                            .ToProtoDict<int, NetMiniGameNKSV2MissionProgress, MiniGameNKSV2MissionProgress>(nksv2Data.MissionProgressData);

            response.MissionProgressList.AddRange(missprodata.Values);
            
            if (nksv2Data.ProgressJson!=null) 
            { response.ProgressJson = nksv2Data.ProgressJson; }
        }
        else
        {
            Nksv2Data nksv2DataNew = new();
            List<EventNKSMissionRecord>? missionlist = GameData.Instance.EventNKSMissionTable.Values
                .Where(x => x.CommonSettingsID == req.NKsId).ToList();
            if (missionlist.Count>0)
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

            user.Nksv2Datas.TryAdd(req.NKsId, nksv2DataNew);

            var missprodata = MiniGameHelper
                             .ToProtoDict<int, NetMiniGameNKSV2MissionProgress, MiniGameNKSV2MissionProgress>(nksv2Data.MissionProgressData);

            response.MissionProgressList.AddRange(missprodata.Values);

        }


        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}