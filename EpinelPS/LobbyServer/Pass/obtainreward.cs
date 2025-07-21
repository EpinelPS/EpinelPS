using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/obtainreward")]
    public class ObtainPassReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainPassReward req = await ReadData<ReqObtainPassReward>(); //fields "PassId", "PassRank"

            ResObtainPassReward response = new(); // field Reward

           
		   await WriteDataAsync(response);
        }
    }
}
