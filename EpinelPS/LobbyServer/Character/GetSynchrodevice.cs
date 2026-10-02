using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/synchrodevice/get")]
public class GetSynchrodevice : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetSynchroData req = await ReadData<ReqGetSynchroData>();
        User user = GetUser();

        if (user.SynchroSlots.Count == 0)
        {

            user.SynchroSlots = [
                new SynchroSlot() { Slot = 1 },
        new SynchroSlot() { Slot = 2},
        new SynchroSlot() { Slot = 3 },
        new SynchroSlot() { Slot = 4 },
        new SynchroSlot() { Slot = 5 },
    ];
        }

        List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

        if (user.SynchroDeviceUpgraded && highestLevelCharacters.Count >= 5)
        {
            int minStandardLevel = highestLevelCharacters.Min(x => x.Level);
            if (user.SynchroDeviceLevel < minStandardLevel)
            {
                user.SynchroDeviceLevel = minStandardLevel;
            }

            while (user.SynchroSlots.Count < 5)
            {
                user.SynchroSlots.Add(new SynchroSlot { Slot = user.SynchroSlots.Count + 1, AvailableAt = 1 });
            }

            for (int i = 0; i < 5; i++)
            {
                int slotNum = i + 1;
                var s = user.SynchroSlots.FirstOrDefault(x => x.Slot == slotNum);
                if (s != null && s.CharacterSerialNumber != highestLevelCharacters[i].Csn)
                {
                    s.CharacterSerialNumber = highestLevelCharacters[i].Csn;
                    s.AvailableAt = 1;
                }
            }
        }

        ResGetSynchroData response = new()
        {
            Synchro = new NetUserSynchroData()
        };

        foreach (CharacterModel? item in highestLevelCharacters)
        {
            int charLv = user.SynchroDeviceUpgraded ? Math.Max(item.Level, user.GetSynchroLevel()) : item.Level;
            response.Synchro.StandardCharacters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = charLv, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });
        }

        foreach (SynchroSlot item in user.SynchroSlots.OrderBy(s => s.Slot))
        {
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = item.Slot, AvailableRegisterAt = item.AvailableAt != 0 ? item.AvailableAt : 1, Csn = item.CharacterSerialNumber });
        }

        int highestLevel = user.Characters.Count > 0 ? user.Characters.Max(c => c.Level) : 0;
        int ownedCount = user.Characters.Count;
        int totalLimitBreaks = user.Characters.Sum(c => Math.Min(c.Grade, 3));
        response.Synchro.SynchroMaxLv = (int)(highestLevel + ownedCount + totalLimitBreaks * 1.334);
        response.Synchro.SynchroLv = user.GetSynchroLevel();
        response.Synchro.IsChanged = user.SynchroDeviceUpgraded;
        await WriteDataAsync(response);
    }
}
