using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/choosecharacter")]
public class ChooseCharacter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        Random random = new Random();
        ReqChooseLiberateCharacter req = await ReadData<ReqChooseLiberateCharacter>();
        User user = GetUser();
        ResChooseLiberateCharacter response = new();
        Logging.WriteLine($"{req.CharacterId}", LogType.Info);
        LiberateCharacterRecord? liberatechar = GameData.Instance.LiberateCharacterTable.Values
            .Where(x=>x.Id== req.CharacterId).FirstOrDefault();

        NetLiberateData libd = new()
        {
            CharacterId = liberatechar.Id,
            IsCompleted = false,
            ProgressPoint = 0,
            RewardedCount = 0,
            Step = 1
        };

        LiberateMissionGroupRecord? missgroup = GameData.Instance.LiberateMissionGroupTable.Values
            .Where(x => x.MissionStepGroup == liberatechar.MissionStepGroup && x.MissionStep == libd.Step).FirstOrDefault();

        List<int>? subGroupIds = GameData.Instance.LiberateMissionTable.Values
            .Where(x => x.GroupId == missgroup.DefaultMissionGroupId)
            .Select(g => g.SubGroupId)
            .Distinct()  // 去重
            .ToList();

        if (subGroupIds != null && subGroupIds.Count > 0)
        {
            int randomSubGroupId = subGroupIds[random.Next(subGroupIds.Count)];
            List<NetLiberateMissionData>? list = LiberateHelper.GetMissions(req.CharacterId, missgroup.DefaultMissionGroupId, randomSubGroupId);
            libd.MissionData.AddRange(list);
        }

        user.LiberateDatas.TryAdd(req.CharacterId, libd);
        user.OpenLiberateTypeIdList.AddUnique(liberatechar.TypeGroupId);
        user.CurCharacterIdId = req.CharacterId;

        response.Data = libd;

        //response.Error = LiberateDataExpiredError.Success;
        JsonDb.Save();
        await WriteDataAsync(response);
    }

    
}