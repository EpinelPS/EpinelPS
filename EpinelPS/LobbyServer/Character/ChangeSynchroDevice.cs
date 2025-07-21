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
            ReqSynchroChange req = await ReadData<ReqSynchroChange>();
            User user = GetUser();

            ResSynchroChange response = new();

            List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

            int slot = 1;
            foreach (CharacterModel? item in highestLevelCharacters)
            {
                if (item.Level != 200)
                {
                    throw new Exception("expected level to be 200");
                }

                response.Characters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });



                foreach (SynchroSlot s in user.SynchroSlots)
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

            foreach (SynchroSlot item in user.SynchroSlots)
            {
                response.Slots.Add(new NetSynchroSlot() { Slot = item.Slot, AvailableRegisterAt = item.AvailableAt, Csn = item.CharacterSerialNumber });
            }
            
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
