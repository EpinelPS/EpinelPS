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

        ResGetSynchroData response = new()
        {
            Synchro = new NetUserSynchroData()
        };

        foreach (CharacterModel? item in highestLevelCharacters)
        {
            response.Synchro.StandardCharacters.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });
        }

        foreach (SynchroSlot item in user.SynchroSlots)
        {
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = item.Slot, AvailableRegisterAt = 1, Csn = item.CharacterSerialNumber });
        }

        // Official formula for Maximum Synchro Level:
        //   Highest Nikke level + (Nikkes owned) + (Total Limit Breaks × 1.334)
        // In EpinelPS a character's Grade encodes: 0-3 = star rank (these ARE
        // the limit breaks), 4-10 = core/overload tiers (do NOT count). So a
        // character's limit breaks = min(Grade, 3): R stays 0★ (0), SR caps at
        // 2★ (2), SSR caps at 3★ (3); core breaks beyond 3★ are excluded.
        int highestLevel = user.Characters.Count > 0 ? user.Characters.Max(c => c.Level) : 0;
        int ownedCount = user.Characters.Count;
        int totalLimitBreaks = user.Characters.Sum(c => Math.Min(c.Grade, 3));
        response.Synchro.SynchroMaxLv = (int)(highestLevel + ownedCount + totalLimitBreaks * 1.334);
        response.Synchro.SynchroLv = user.GetSynchroLevel();
        response.Synchro.IsChanged = user.SynchroDeviceUpgraded;

        await WriteDataAsync(response);
    }
}
