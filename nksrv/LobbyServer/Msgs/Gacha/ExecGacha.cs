using nksrv.StaticInfo;
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
            var user = GetUser();

            var response = new ResExecuteGacha();

            // TODO: Pick random character that player does not have unless it supports limit break.

            // TODO implement reward
            response.Reward = new NetRewardData();

            if (user.GachaTutorialPlayCount == 0)
            {
                foreach (var c in StaticDataParser.Instance.GetAllCharacterTids())
                {
                    response.Gacha.Add(new NetGachaEntityData() { Corporation = 0, PieceCount = 1, CurrencyValue = 5, Sn = 130201, Tid = c, Type = 1 });

                    user.Characters.Add(new Utils.Character() { CostumeId = 0, Csn = c, Grade = 0, Level = 1, Skill1Lvl = 1, Skill2Lvl = 1, Tid = c, UltimateLevel = 1 });

                    //  response.Characters.Add(new NetUserCharacterDefaultData() { Lv = 1, Skill1Lv = 1, Grade = 0, Csn = 1, Tid = 130201 });

                }
                user.GachaTutorialPlayCount++;
            }

            JsonDb.Save();
           

          await  WriteDataAsync(response);
        }
    }
}
