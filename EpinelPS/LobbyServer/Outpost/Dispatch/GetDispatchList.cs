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

        if (user.ResetableData.Dispatches.Count == 0)
        {
            user.DispatchClearList = new();
            user.SelectableDispatchData = new();
            user.DispatchResetCount = 0;

            //记录已选中的任务
            List<DispatchRecord> availableDispatchTable = new List<DispatchRecord>();


            void GetAvailableDispatches(DispatchBoardRecord records)
            {
                for (int i = 0; i < records.DispatchMax; i++)
                {
                    var list = records.DispatchList;

                    DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(list, x => x.DispatchProb);
                    List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                        .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                    dispatchtable = dispatchtable.Where(x => !availableDispatchTable.Any(d => d.Id == x.Id))
                        .ToList();

                    int randomNumber = random.Next(0, dispatchtable.Count);
                    availableDispatchTable.Add(dispatchtable[randomNumber]);
                    DispatchData netUserDispatch = new DispatchData
                    {
                        TableId = dispatchtable[randomNumber].Id,
                        Running = false,
                        StartAt = startTime,
                        EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin)
                    };
                    user.ResetableData.Dispatches.Add(netUserDispatch);
                    response.DispatchList.Add(netUserDispatch.ToNet());
                }
            }

            for (int i = 0; i < user.ResetableData.DispatchCount; i++)
            {
                DispatchBoardData groudid = DispatchHelper.SelectItemByProbability(dispatch, x => x.DispatchProb);


                List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                    .Where(x => x.DispatchGroup == groudid.DispatchGroup).ToList();

                // 从dispatchtable中移除已选择的Tid
                dispatchtable = dispatchtable.Where(x => !availableDispatchTable.Any(d => d.Id == x.Id)).ToList();

                int randomNumber = random.Next(0, dispatchtable.Count);

                availableDispatchTable.Add(dispatchtable[randomNumber]);
                DispatchData netUserDispatch = new DispatchData
                {
                    TableId = dispatchtable[randomNumber].Id,
                    Running = false,
                    StartAt = startTime,
                    EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin)
                };

                user.ResetableData.Dispatches.Add(netUserDispatch);
                response.DispatchList.Add(netUserDispatch.ToNet());
            }

            GetAvailableDispatches(dispatchcol);
            GetAvailableDispatches(dispatchfav);
        }
        else
        {
            foreach (var item in user.ResetableData.Dispatches)
                response.DispatchList.Add(item.ToNet());
        }

        //List<NetSelectableDispatchData> dontdispatcht = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        response.DispatchResetCount = user.DispatchResetCount;
        //response.SelectableDispatchList.AddRange(dontdispatcht);
        JsonDb.Save();
        await GameContext.SaveChangesAsync();
        // TODO
        await WriteDataAsync(response);
    }
}