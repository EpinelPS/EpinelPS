using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/SynchroDevice/Unregist")]
    public class UnregisterSynchroDevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSynchroUnregist req = await ReadData<ReqSynchroUnregist>();
            User user = GetUser();

            ResSynchroUnregist response = new();

            foreach (SynchroSlot item in user.SynchroSlots)
            {
                if (item.Slot == req.Slot)
                {
                    if (item.CharacterSerialNumber == 0)
                    {
                        Logging.WriteLine("must add character from synchrodevice first", LogType.Warning);
                    }
                    else
                    {
                        long oldCSN = item.CharacterSerialNumber;
                        item.CharacterSerialNumber = 0;
                        CharacterModel data = user.GetCharacterBySerialNumber(oldCSN) ?? throw new Exception("failed to lookup character");

                        response.Character = new NetUserCharacterDefaultData()
                        {
                            Csn = data.Csn,
                            CostumeId = data.CostumeId,
                            Grade = data.Grade,
                            Lv = data.Level,
                            Skill1Lv = data.Skill1Lvl,
                            Skill2Lv = data.Skill2Lvl,
                            Tid = data.Tid,
                            UltiSkillLv = data.UltimateLevel
                        };
                        response.Slot = new NetSynchroSlot() { AvailableRegisterAt = item.AvailableAt, Csn = item.CharacterSerialNumber, Slot = item.Slot };

                        response.IsSynchro = false;
                        List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];


                        foreach (CharacterModel? item2 in highestLevelCharacters)
                        {
                            response.SynchroStandardCharacters.Add(item2.Csn);
                        }

                        response.SynchroLv = user.GetSynchroLevel();
                    }
                }
            }

            JsonDb.Save();


            await WriteDataAsync(response);
        }
    }
}
