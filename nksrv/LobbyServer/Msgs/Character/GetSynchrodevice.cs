using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/synchrodevice/get")]
    public class GetSynchrodevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSynchroData>();
            var user = GetUser();

            var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

            var response = new ResGetSynchroData();
            response.Synchro = new NetUserSynchroData();

            foreach (var item in highestLevelCharacters)
            {
                response.Synchro.StandardCharacters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel } });
            }
        
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 1 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 2 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 3 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 4 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 5 });

            response.Synchro.SynchroMaxLv = highestLevelCharacters.First().Level;
            response.Synchro.SynchroLv = highestLevelCharacters.Last().Level;

            // TODO: Validate response from real server and pull info from user info
            WriteData(response);
        }
    }
}
