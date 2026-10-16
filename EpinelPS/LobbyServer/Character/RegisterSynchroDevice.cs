using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/SynchroDevice/Regist")]
public class RegisterSynchroDevice : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSynchroRegister req = await ReadData<ReqSynchroRegister>();
        User user = GetUser();
        CharacterModel? targetCharacter = user.GetCharacterBySerialNumber(req.Csn) ?? throw new Exception("target character does not exist");
        ResSynchroRegister response = new();
        if (user.SynchroDeviceUpgraded && req.Slot <= 5)
        {
            Console.WriteLine("Cannot replace locked standard character in enhanced synchro device");
            await WriteDataAsync(response);
            return;
        }

        foreach (SynchroSlot item in user.SynchroSlots)
        {
            if (item.Slot == req.Slot)
            {
                if (item.CharacterSerialNumber != 0)
                {
                    Console.WriteLine("must remove character from synchrodevice first");
                }
                else
                {
                    item.CharacterSerialNumber = req.Csn;
                    response.IsSynchro = true;
                    response.Character = new NetUserCharacterDefaultData()
                    {
                        Csn = item.CharacterSerialNumber,
                        CostumeId = targetCharacter.CostumeId,
                        Grade = targetCharacter.Grade,
                        Lv = targetCharacter.Level,
                        Skill1Lv = targetCharacter.Skill1Lvl,
                        Skill2Lv = targetCharacter.Skill2Lvl,
                        Tid = targetCharacter.Tid,
                        UltiSkillLv = targetCharacter.UltimateLevel
                    };
                    response.Slot = new NetSynchroSlot() { AvailableRegisterAt = item.AvailableAt != 0 ? item.AvailableAt : 1, Csn = item.CharacterSerialNumber, Slot = item.Slot };
                }
            }
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
