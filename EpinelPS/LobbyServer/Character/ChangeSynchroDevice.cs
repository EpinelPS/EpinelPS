using EpinelPS.Database;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/SynchroDevice/Change")]
    public class ChangeSynchroDevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSynchroChange>();
            var user = GetUser();

            var response = new ResSynchroChange();

            var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

            int slot = 1;
            foreach (var item in highestLevelCharacters)
            {
                if (item.Level != 200)
                {
                    throw new Exception("expected level to be 200");
                }

                response.Characters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Level = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });



                foreach (var s in user.SynchroSlots)
                {
                    if (s.Slot == slot)
                    {
                        s.CharacterSerialNumber = item.Csn;
                        break;
                    }
                }
                slot++;
            }

            user.SynchroDeviceUpgraded = true;

            foreach (var item in user.SynchroSlots)
            {
                response.Slots.Add(new NetSynchroSlot() { Slot = item.Slot, AvailableRegisterAt = item.AvailableAt, Csn = item.CharacterSerialNumber });
            }
            
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
