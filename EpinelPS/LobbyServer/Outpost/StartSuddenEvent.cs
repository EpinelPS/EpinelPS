using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/suddenevent/start")]
public class StartSuddenEvent : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStartSuddenEvent req = await ReadData<ReqStartSuddenEvent>();
        User user = GetUser();

        var suddenEvent = GameData.Instance.SuddenEventConditions[req.Tid];
        if (user.ClearedOutpostScenarioIds.Contains(suddenEvent.Id)) throw new InvalidOperationException();

        if (!user.CanSubtractCurrency(CurrencyType.ContentStamina, 1))
        {
            throw new InvalidOperationException("Not enough stamina");
        }

        user.SubtractCurrency(CurrencyType.ContentStamina, 1);

        ResStartSuddenEvent response = new()
        {
            Currency = new()
            {
                Type = (int)CurrencyType.ContentStamina,
                Value = user.GetCurrencyVal(CurrencyType.ContentStamina)
            },
            Reward = RewardUtils.RegisterRewardsForUser(user, GameData.Instance.GetRewardTableEntry(suddenEvent.RewardId))
        };

        user.ClearedOutpostScenarioIds.Add(suddenEvent.Id);

        await WriteDataAsync(response);
    }
}
