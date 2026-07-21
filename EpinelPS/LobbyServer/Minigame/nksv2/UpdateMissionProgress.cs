using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/updatemissionprogress")]
public class UpdateMissionProgress : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUpdateNKSV2MissionProgress req = await ReadData<ReqUpdateNKSV2MissionProgress>();
        User user = GetUser();
        ResUpdateNKSV2MissionProgress response = new();

        //Logging.WriteLine($"{req.NKsId},{req.ProgressUpdateList}", LogType.Info);

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            if (req.ProgressUpdateList.Count >0)
            {
                foreach (var item in req.ProgressUpdateList)
                {
                    var newdata = nksv2Data.MissionProgressData.Values.FirstOrDefault(x => x.Seq == item.SeqId);
                    if (newdata != null)
                    {
                        newdata.Progress = item.ProgressCount;  
                    }
                }
            }


            var missprodata = MiniGameHelper
                             .ToProtoDict<int, NetMiniGameNKSV2MissionProgress, MiniGameNKSV2MissionProgress>(nksv2Data.MissionProgressData);

            response.MissionProgressList.AddRange(missprodata.Values);
            response.Error = NKSV2MissionExpiredError.Succeed;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}