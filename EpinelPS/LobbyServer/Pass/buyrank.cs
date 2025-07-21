using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/buyrank")]
    public class BuyPassRank : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBuyPassRank req = await ReadData<ReqBuyPassRank>(); //fields "PassId", "TargetPassRank"

            ResBuyPassRank response = new(); // fields "PassRank", "PassPoint", "Currencies"

           
		   await WriteDataAsync(response);
        }
    }
}
