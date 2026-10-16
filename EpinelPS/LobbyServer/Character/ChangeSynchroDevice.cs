using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/SynchroDevice/Change")]
public class ChangeSynchroDevice : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSynchroChange req = await ReadData<ReqSynchroChange>();
        User user = GetUser();

        ResSynchroChange response = new();

        List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

        if (highestLevelCharacters.Count < 5 || highestLevelCharacters.Any(x => x.Level < 200))
        {
            Logging.WriteLine("Synchro change rejected: requires 5 characters of level 200 or higher", LogType.Warning);
            await WriteDataAsync(response);
            return;
        }

        int minStandardLevel = highestLevelCharacters.Min(x => x.Level);
        user.SynchroDeviceLevel = Math.Max(user.SynchroDeviceLevel, minStandardLevel);
        user.SynchroDeviceUpgraded = true;

        // Ensure at least 5 slots exist
        while (user.SynchroSlots.Count < 5)
        {
            user.SynchroSlots.Add(new SynchroSlot { Slot = user.SynchroSlots.Count + 1, AvailableAt = 1 });
        }

        // Preserve any existing non-standard characters in slots 1-5
        var top5Csns = highestLevelCharacters.Select(h => (long)h.Csn).ToHashSet();
        List<long> displacedCsns = user.SynchroSlots
            .Where(s => s.Slot <= 5 && s.CharacterSerialNumber != 0 && !top5Csns.Contains(s.CharacterSerialNumber))
            .Select(s => s.CharacterSerialNumber)
            .ToList();

        // Lock standard characters into slots 1-5
        for (int i = 0; i < 5; i++)
        {
            int slotNum = i + 1;
            SynchroSlot? targetSlot = user.SynchroSlots.FirstOrDefault(s => s.Slot == slotNum);
            if (targetSlot != null)
            {
                targetSlot.CharacterSerialNumber = highestLevelCharacters[i].Csn;
                targetSlot.AvailableAt = 1;
            }
        }

        // Shift any displaced characters to slots 6+
        foreach (long displacedCsn in displacedCsns)
        {
            var emptySlot = user.SynchroSlots.FirstOrDefault(s => s.Slot > 5 && s.CharacterSerialNumber == 0);
            if (emptySlot != null)
            {
                emptySlot.CharacterSerialNumber = displacedCsn;
                emptySlot.AvailableAt = 1;
            }
            else
            {
                int newSlotNum = user.SynchroSlots.Max(s => s.Slot) + 1;
                user.SynchroSlots.Add(new SynchroSlot { Slot = newSlotNum, CharacterSerialNumber = displacedCsn, AvailableAt = 1 });
            }
        }

        foreach (CharacterModel item in highestLevelCharacters)
        {
            item.Level = Math.Max(item.Level, user.SynchroDeviceLevel);

            response.Characters.Add(new NetUserCharacterData()
            {
                Default = new()
                {
                    Csn = item.Csn,
                    Skill1Lv = item.Skill1Lvl,
                    Skill2Lv = item.Skill2Lvl,
                    CostumeId = item.CostumeId,
                    Lv = item.Level,
                    Grade = item.Grade,
                    Tid = item.Tid,
                    UltiSkillLv = item.UltimateLevel
                },
                IsSynchro = user.GetSynchro(item.Csn)
            });
        }

        foreach (SynchroSlot item in user.SynchroSlots.OrderBy(s => s.Slot))
        {
            response.Slots.Add(new NetSynchroSlot()
            {
                Slot = item.Slot,
                AvailableRegisterAt = item.AvailableAt != 0 ? item.AvailableAt : 1,
                Csn = item.CharacterSerialNumber
            });
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
