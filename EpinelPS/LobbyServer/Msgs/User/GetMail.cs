using EpinelPS.Utils;
using Swan;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/mail/get")]
    public class GetMail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMailData>();

            var r = new ResGetMailData();
            r.Mail.Add(new NetUserMailData()
            {

                Msn = 3,
                Nickname = "nick",
                Title = new() { IsPlain = true, Str = "Our Server Fell" },
                Text = new() { IsPlain = true, Str = "Our Game Was Down For 1 Second because the HP Laptop Which Hosted the Server Got Shut Down by the Lid Closing. As for the Reward for the Inconvience, free paid gems" },
                HasReward = true,
                Sender = 102

            });

            r.Mail[0].Items.Add(new NetMailRewardItem() { ExpiredAt = DateTime.UtcNow.AddYears(10).ToUnixEpochDate(), RewardId = 1, RewardType = (int)CurrencyType.ChargeCash, RewardValue = 100000 });

            await WriteDataAsync(r);
        }
    }
}
