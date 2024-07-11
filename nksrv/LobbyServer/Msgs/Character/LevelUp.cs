using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/levelup")]
    public class LevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterLevelUp>();
            var user = GetUser();
            var response = new ResCharacterLevelUp();

            foreach (var item in user.Characters.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    item.Level = req.Level;
                    // TODO: subtract currency


                    response.Character = new() { 
                        CostumeId = item.CostumeId, 
                        Csn = item.Csn,
                        Lv = item.Level,
                        Skill1Lv = item.Skill1Lvl,
                        Skill2Lv = item.Skill2Lvl,
                        UltiSkillLv = item.UltimateLevel,
                        Grade = item.Grade,
                        Tid = item.Tid
                    };
                    var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

                    response.SynchroLv = highestLevelCharacters.Last().Level;

                    foreach (var c in highestLevelCharacters)
                    {
                        response.SynchroStandardCharacters.Add(c.Tid);
                    }

                    break;
                }
            }
            JsonDb.Save();

         

          await  WriteDataAsync(response);
        }
    }
}
