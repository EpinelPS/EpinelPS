using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/Outpost/ResetDispatchList")]
public class ResetDispatchList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetDispatchList req = await ReadData<ReqResetDispatchList>();

        ResResetDispatchList response = new();
        User user = GetUser();
        DateTime startTime = DateTime.UtcNow;
        Random random = new();
        user.DispatchResetCount++;

        var reset = GameData.Instance.DispatchResetTable.Values.Where(x => x.Id == user.DispatchResetCount).FirstOrDefault();
        if (reset != null)
        {
            response.Currencies.Add(new NetUserCurrencyData() { Type = (int)reset.CurrencyType, Value = reset.CurrencyValue });
        }
        else
        {
            response.Currencies.Add(new NetUserCurrencyData() { Type = (int)CurrencyType.FreeCash, Value = 50 });
        }

        List<NetUserDispatchData> list = user.UserDispatchData.dispatchDatas.Where(x => x.IsRun == 0).ToList();
        List<NetUserDispatchData> olist = user.UserDispatchData.dispatchDatas.Where(x => x.IsRun == 1).ToList();

        List<int> runlist = user.UserDispatchData.dispatchDatas.Select(x => x.Tid).ToList();


        if (list.Count > 0)
        {
            foreach (NetUserDispatchData data in list)
            {
                DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values.Where(c => c.Id == data.Tid).FirstOrDefault();
                List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                    .Where(x => x.DispatchGroup == dispatch.DispatchGroup).ToList();
                dispatchtable = dispatchtable.Where(x => !runlist.Contains(x.Id)).ToList();

                int randomNumber = random.Next(0, dispatchtable.Count);

                runlist.Add(dispatchtable[randomNumber].Id);
                NetUserDispatchData netUserDispatch = new NetUserDispatchData
                {
                    Tid = dispatchtable[randomNumber].Id,
                    IsRun = 0,
                    StartAt = startTime.Ticks,
                    EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin).Ticks
                };

                user.UserDispatchData.dispatchDatas.Add(netUserDispatch);
                user.UserDispatchData.dispatchDatas.RemoveAll(x => x.Tid == data.Tid);
                runlist.Remove(data.Tid);
                response.DispatchList.Add(netUserDispatch);

            }
        }

        List<NetSelectableDispatchData> dontdispatcht = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        response.DispatchList.AddRange(olist);
        response.DispatchResetCount = user.DispatchResetCount;
        await GameContext.SaveChangesAsync();
        //response.SelectableDispatchList.AddRange(dontdispatcht);
        await WriteDataAsync(response);
    }
}