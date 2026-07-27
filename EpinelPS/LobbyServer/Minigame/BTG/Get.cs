using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBtgData req = await ReadData<ReqGetArcadeBtgData>();
        User user = GetUser();
        ResGetArcadeBtgData response = new();

        if (user.BtgData.TryGetValue(req.BtgId,out var btgData))
        {
            response.BtgData = MiniGameHelper.ToProto<NetArcadeBtgData, BtgData>(btgData.Data);
        }
        else
        {
            BtgData netArcadeBtgData = new BtgData() 
            {
                BtgId = req.BtgId
            };

            List<EventBTGMissionRecord_Raw>? missionlist = GameData.Instance.EventBTGMissionTable.Values
                .Where(x=>x.ManagerId == req.BtgId).ToList();

            Dictionary<int, MiniGameBtgMissionData> missionDatas = [];

            if (missionlist.Count > 0)
            {
                foreach (var item in missionlist)
                {
                    missionDatas.TryAdd(item.Id,new()
                    {
                        MissionId = item.Id,
                        Progress = 0,
                        IsReceived = false,
                    });
                }
            }


            user.BtgData.TryAdd(req.BtgId, new ArcadeBtgData()
            { 
                Data = netArcadeBtgData,
                MissionDatas = missionDatas
            });

            response.BtgData = MiniGameHelper.ToProto<NetArcadeBtgData, BtgData>(netArcadeBtgData); ;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}