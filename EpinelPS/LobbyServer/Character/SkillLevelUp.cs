using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/skill/levelup")]
    public class SkillLevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterSkillLevelUp>();
            var user = GetUser();
            var response = new ResCharacterSkillLevelUp();
            
            var character = user.Characters.FirstOrDefault(c => c.Csn == req.Csn) ?? throw new Exception("cannot find character");

            var charRecord = GameData.Instance.CharacterTable.Values.FirstOrDefault(c => c.id == character.Tid) ?? throw new Exception("cannot find character record");
            var skillIdMap = new Dictionary<int, int>
            {
                { 1, charRecord.ulti_skill_id },
                { 2, charRecord.skill1_id },
                { 3, charRecord.skill2_id }
            };
            var skillLevelMap = new Dictionary<int, int>
            {
                { 1, character.UltimateLevel },
                { 2, character.Skill1Lvl },
                { 3, character.Skill2Lvl }
            };
            var skillRecord = GameData.Instance.skillInfoTable.Values.FirstOrDefault(s => s.id == skillIdMap[req.Category] + (skillLevelMap[req.Category] - 1)) ?? throw new Exception("cannot find character skill record");
            var costRecord = GameData.Instance.costTable.Values.FirstOrDefault(c => c.id == skillRecord.level_up_cost_id) ?? throw new Exception("cannot find character cost record");

            foreach (var cost in costRecord.costs.Where(i => i.item_type != "None"))
            {
                var item = user.Items.FirstOrDefault(i => i.ItemType == cost.item_id) ?? throw new NullReferenceException();

                item.Count -= cost.item_value;

                response.Items.Add(new NetUserItemData
                {
                    Isn = item.Isn,
                    Tid = cost.item_id,
                    Count = item.Count,
                    Csn = item.Csn,
                    Corporation = item.Corp,
                    Lv = item.Level,
                    Exp = item.Exp,
                    Position = item.Position
                });
            }

            var newChar = new NetUserCharacterDefaultData
            {
                CostumeId = character.CostumeId,
                Csn = character.Csn,
                Lv = character.Level,
                Grade = character.Grade,
                Tid = character.Tid,
                DispatchTid = character.Tid,
                Skill1Lv = character.Skill1Lvl,
                Skill2Lv = character.Skill2Lvl,
                UltiSkillLv = character.UltimateLevel,
            };

            if (req.Category == 1)
            {
                character.UltimateLevel++;
                newChar.UltiSkillLv++;
            }
            else if (req.Category == 2)
            {
                character.Skill1Lvl++;
                newChar.Skill1Lv++;
            }
            else if (req.Category == 3)
            {
                character.Skill2Lvl++;
                newChar.Skill2Lv++;
            }

            if (character.UltimateLevel == 10 && character.Skill1Lvl == 10 && character.Skill2Lvl == 10)
            {
                user.AddTrigger(TriggerType.CharacterSkillLevelMax, 1);
            }

            response.Character = newChar;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
