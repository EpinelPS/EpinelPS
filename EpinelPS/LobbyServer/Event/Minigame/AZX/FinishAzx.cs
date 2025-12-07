using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/finish")]
    public class FinishAzx : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "azxId": 1, "scoreAndTime": { "score": 50000, "timeToScore": "110.122697s" }, 
            //  "playBoardId": 101, "playCharacterId": 101, "skillUseCountList": [ { "skillId": 102 } ], "cutSceneId": 10101 }
            ReqFinishMiniGameAzx req = await ReadData<ReqFinishMiniGameAzx>();
            User user = GetUser();
            
            ResFinishMiniGameAzx response = new();

            AzxHelper.FinishAzx(user, req, ref response);

            await WriteDataAsync(response);
        }
    }
}