using EpinelPS.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/SynchroDevice/Regist")]
    public class RegisterSynchroDevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSynchroRegister>();
            var user = GetUser();
            var targetCharacter = user.GetCharacterBySerialNumber(req.Csn);
            if (targetCharacter == null) throw new Exception("target character does not exist");

            var response = new ResSynchroRegister();
            foreach (var item in user.SynchroSlots)
            {
                if (item.Slot == req.Slot)
                {
                    if (item.CharacterSerialNumber != 0)
                    {
                        Logger.Warn("must remove character from synchrodevice first");
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
                            Level = user.GetSynchroLevel(),
                            Skill1Lv = targetCharacter.Skill1Lvl,
                            Skill2Lv = targetCharacter.Skill2Lvl,
                            Tid = targetCharacter.Tid,
                            UltiSkillLv = targetCharacter.UltimateLevel
                        };
                        response.Slot = new NetSynchroSlot() { AvailableRegisterAt = item.AvailableAt, Csn = item.CharacterSerialNumber, Slot = item.Slot };
                    }
                }
            }
           

            await WriteDataAsync(response);
        }
    }
}
