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

        var notRunning = user.ResetableData.Dispatches.Where(x => !x.Running).ToList();
        var running = user.ResetableData.Dispatches.Where(x => x.Running).ToList();

        List<int> runlist = user.ResetableData.Dispatches.Select(x => x.TableId).ToList();

        if (notRunning.Count > 0)
        {
            foreach (var data in notRunning)
            {
                DispatchRecord? dispatch = GameData.Instance.DispatchTable.Values.Where(c => c.Id == data.TableId).FirstOrDefault();
                List<DispatchRecord> dispatchtable = GameData.Instance.DispatchTable.Values
                    .Where(x => x.DispatchGroup == dispatch.DispatchGroup).ToList();
                dispatchtable = dispatchtable.Where(x => !runlist.Contains(x.Id)).ToList();

                int randomNumber = random.Next(0, dispatchtable.Count);

                runlist.Add(dispatchtable[randomNumber].Id);
                DispatchData netUserDispatch = new DispatchData
                {
                    TableId = dispatchtable[randomNumber].Id,
                    Running = false,
                    StartAt = startTime,
                    EndAt = startTime.AddMinutes(dispatchtable[randomNumber].TimeMin)
                };

                user.ResetableData.Dispatches.Add(netUserDispatch);
                user.ResetableData.Dispatches.RemoveAll(x => x.TableId == data.TableId);
                runlist.Remove(data.TableId);
                response.DispatchList.Add(netUserDispatch.ToNet());

            }
        }

        //List<NetSelectableDispatchData> dontdispatcht = user.SelectableDispatchData.Where(x => user.DispatchClearList.Contains(x.SelectTid)).ToList();

        foreach (var item in running)
            response.DispatchList.Add(item.ToNet());
        response.DispatchResetCount = user.DispatchResetCount;
        await GameContext.SaveChangesAsync();
        //response.SelectableDispatchList.AddRange(dontdispatcht);
        await WriteDataAsync(response);
    }
}