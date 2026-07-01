using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/outpost/dispatch/get")]
public class GetDispatchList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetDispatchList req = await ReadData<ReqGetDispatchList>();
        Random random = new Random();
        ResGetDispatchList response = new();

        User user = GetUser();

        List<DispatchBoardData> dispatch = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.Dispatch && x.DispatchBoardLv == user.DispatchLv)
            .FirstOrDefault().DispatchList;
        DispatchBoardRecord? dispatchcol = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.DispatchCollection &&
                        x.DispatchBoardLv == user.DispatchCollectionLv).FirstOrDefault();
        DispatchBoardRecord? dispatchfav = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.DispatchFavorite && x.DispatchBoardLv == user.DispatchFavoriteLv)
            .FirstOrDefault();

        DateTime startTime = DateTime.UtcNow;
        int dateDay = user.GetDateDay();

        if (user.UserDispatchData.Today != dateDay)
        {
            user.UserDispatchData.Today = dateDay;
            user.UserDispatchData.dispatchDatas = new();
            user.DispatchClearList = new();
            user.SelectableDispatchData = new();
            user.DispatchResetCount = 0;

            //记录已选中的任务
            List<DispatchRecord> availableDispatchTable = new List<DispatchRecord>();

            for (int i = 0; i < user.ResetableData.DispatchCount; i++)
            {
                DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(dispatch, x => x.DispatchProb);


                List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                    .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                // 从dispatchtable中移除已选择的Tid
                dispatchtable = dispatchtable.Where(x => !availableDispatchTable.Any(d => d.Id == x.Id)).ToList();

                int randomNumber = random.Next(0, dispatchtable.Count);

                availableDispatchTable.Add(dispatchtable[randomNumber]);
                NetUserDispatchData netUserDispatch = new NetUserDispatchData
                {
                    Tid = dispatchtable[randomNumber].Id,
                    IsRun = 0,
                    StartAt = startTime.Ticks,
                    EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin).Ticks
                    //EndAt = startTime.AddSeconds(dispatchtable[randomNumber].TimeMin).Ticks
                };

                user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                response.DispatchList.Add(netUserDispatch);
            }

            if (dispatchcol != null)
            {
                for (int i = 0; i < dispatchcol.DispatchMax; i++)
                {
                    var list = dispatchcol.DispatchList;

                    DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(list, x => x.DispatchProb);
                    List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                        .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                    // 从dispatchtable中移除已选择的Tid
                    dispatchtable = dispatchtable.Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                        .ToList();

                    int randomNumber = random.Next(0, dispatchtable.Count);
                    availableDispatchTable.Add(dispatchtable[randomNumber]);
                    NetUserDispatchData netUserDispatch = new NetUserDispatchData
                    {
                        Tid = dispatchtable[randomNumber].Id,
                        IsRun = 0,
                        StartAt = startTime.Ticks,
                        EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin).Ticks
                        //EndAt = startTime.AddSeconds(dispatchtable[randomNumber].TimeMin).Ticks
                    };
                    user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                    response.DispatchList.Add(netUserDispatch);
                }
            }

            if (dispatchfav != null)
            {
                for (int i = 0; i < dispatchfav.DispatchMax; i++)
                {
                    var list = dispatchfav.DispatchList;

                    DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(list, x => x.DispatchProb);
                    List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                        .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                    // 从dispatchtable中移除已选择的Tid
                    dispatchtable = dispatchtable.Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                        .ToList();

                    int randomNumber = random.Next(0, dispatchtable.Count);
                    availableDispatchTable.Add(dispatchtable[randomNumber]);
                    NetUserDispatchData netUserDispatch = new NetUserDispatchData
                    {
                        Tid = dispatchtable[randomNumber].Id,
                        IsRun = 0,
                        StartAt = startTime.Ticks,
                        EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin).Ticks
                        //EndAt = startTime.AddSeconds(dispatchtable[randomNumber].TimeMin).Ticks
                    };
                    user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                    response.DispatchList.Add(netUserDispatch);
                }
            }
        }
        else
        {
            response.DispatchList.AddRange(user.UserDispatchData.dispatchDatas);
        }

        List<NetSelectableDispatchData> dontdispatcht = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        response.DispatchResetCount = user.DispatchResetCount;
        //response.SelectableDispatchList.AddRange(dontdispatcht);
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}