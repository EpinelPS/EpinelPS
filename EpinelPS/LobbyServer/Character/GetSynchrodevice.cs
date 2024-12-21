using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/synchrodevice/get")]
    public class GetSynchrodevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSynchroData>();
            var user = GetUser();

            if (user.SynchroSlots.Count == 0)
            {

                user.SynchroSlots = new()   {
                    new SynchroSlot() { Slot = 1 },
            new SynchroSlot() { Slot = 2},
            new SynchroSlot() { Slot = 3 },
            new SynchroSlot() { Slot = 4 },
            new SynchroSlot() { Slot = 5 },

             new SynchroSlot() { Slot = 6 },
              new SynchroSlot() { Slot = 7 },
               new SynchroSlot() { Slot = 8 },
                new SynchroSlot() { Slot = 9 },
                 new SynchroSlot() { Slot = 10 },
        };
            }

            var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

            var response = new ResGetSynchroData();
            response.Synchro = new NetUserSynchroData();

            foreach (var item in highestLevelCharacters)
            {
                response.Synchro.StandardCharacters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Level = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });
            }

            foreach (var item in user.SynchroSlots)
            {
                response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = item.Slot, AvailableRegisterAt = 1, Csn = item.CharacterSerialNumber });
            }

            response.Synchro.SynchroMaxLv = 1000;
            response.Synchro.SynchroLv = user.GetSynchroLevel();
            response.Synchro.IsChanged = user.SynchroDeviceUpgraded;

            await WriteDataAsync(response);
        }
    }
}
