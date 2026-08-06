using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/synchrodevice/registoneclick")]
public class SynchroRegisterOneClick : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSynchroRegisterOneClick req = await ReadData<ReqSynchroRegisterOneClick>();
        User user = GetUser();
        ResSynchroRegisterOneClick response = new();

        foreach (NetOneClickSlot oneClickSlot in req.Slots)
        {
            foreach (SynchroSlot slot in user.SynchroSlots)
            {
                if (slot.Slot == oneClickSlot.Slot)
                {
                    if (slot.CharacterSerialNumber == 0)
                    {
                        slot.CharacterSerialNumber = oneClickSlot.Csn;
                    }
                    break;
                }
            }
        }

        JsonDb.Save();

        foreach (SynchroSlot slot in user.SynchroSlots)
        {
            response.Slots.Add(new NetSynchroSlot() { Slot = slot.Slot, AvailableRegisterAt = 1, Csn = slot.CharacterSerialNumber });
        }

        foreach (SynchroSlot slot in user.SynchroSlots)
        {
            if (slot.CharacterSerialNumber == 0) continue;
            CharacterModel? character = user.GetCharacterBySerialNumber(slot.CharacterSerialNumber);
            if (character == null) continue;
            response.Characters.Add(new NetUserCharacterDefaultData()
            {
                Csn = character.Csn,
                CostumeId = character.CostumeId,
                Grade = character.Grade,
                Lv = user.GetSynchroLevel(),
                Skill1Lv = character.Skill1Lvl,
                Skill2Lv = character.Skill2Lvl,
                Tid = character.Tid,
                UltiSkillLv = character.UltimateLevel
            });
        }

        response.IsSynchro = true;

        await WriteDataAsync(response);
    }
}
