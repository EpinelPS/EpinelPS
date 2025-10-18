using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/buyrank")]
    public class BuyEventPassRank : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "passId": 1037, "targetPassRank": 2 }
            ReqBuyEventPassRank req = await ReadData<ReqBuyEventPassRank>(); //fields "PassId", "TargetPassRank"
            User user = GetUser();

            ResBuyEventPassRank response = new(); // fields "PassRank", "PassPoint", "Currencies"
            PassHelper.BuyRank(user, req.PassId, req.TargetPassRank, out int PassPoint, out NetUserCurrencyData currencie);
            response.PassRank = req.TargetPassRank;
            response.PassPoint = PassPoint;
            response.Currencies.Add(currencie);
            
            await WriteDataAsync(response);
        }
    }
}
