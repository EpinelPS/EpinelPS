using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/buyrank")]
    public class BuyPassRank : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBuyPassRank>(); //fields "PassId", "TargetPassRank"
			
            var response = new ResBuyPassRank(); // fields "PassRank", "PassPoint", "Currencies"

           
		   await WriteDataAsync(response);
        }
    }
}
