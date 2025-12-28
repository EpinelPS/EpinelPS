using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost;

[PacketPath("/outpost/suddenevent/start")]
public class StartSuddenEvent : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqStartSuddenEvent req = await ReadData<ReqStartSuddenEvent>();
        User user = GetUser();

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
            }
        };

        await WriteDataAsync(response);
    }
}
