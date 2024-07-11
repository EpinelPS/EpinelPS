using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame.PlaySoda
{
    [PacketPath("/event/minigame/playsoda/challenge/getinfo")]
    public class GetChallengeInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetPlaySodaChallengeModeInfo>();

            var response = new ResGetPlaySodaChallengeModeInfo();
            // TODO
          await  WriteDataAsync(response);
        }
    }
}
