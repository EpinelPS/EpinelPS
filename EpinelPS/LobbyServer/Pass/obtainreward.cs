using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/obtainreward")]
    public class ObtainPassReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainPassReward>(); //fields "PassId", "PassRank"
			
            var response = new ResObtainPassReward(); // field Reward

           
		   await WriteDataAsync(response);
        }
    }
}
