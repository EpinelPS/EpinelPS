using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/execute")]
    public class ExecGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExecuteGacha>();

            var response = new ResExecuteGacha();

            // TODO implement
            response.Reward = new NetRewardData();
            for (int i = 0; i < 10; i++)
            {
                response.Gacha.Add(new NetGachaEntityData() { Corporation = 0, PieceCount = 1, CurrencyValue = 5, Sn = 130201, Tid = 2500601, Type = 1 });
              //  response.Characters.Add(new NetUserCharacterDefaultData() { Lv = 1, Skill1Lv = 1, Grade = 0, Csn = 1, Tid = 130201 });

            }
           

            WriteData(response);
        }
    }
}
