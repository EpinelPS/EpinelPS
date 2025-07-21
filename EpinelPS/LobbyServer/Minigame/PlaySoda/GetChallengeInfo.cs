using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/event/minigame/playsoda/challenge/getinfo")]
    public class GetChallengeInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetPlaySodaChallengeModeInfo req = await ReadData<ReqGetPlaySodaChallengeModeInfo>();

            ResGetPlaySodaChallengeModeInfo response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
