﻿using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/skill/levelup")]
    public class SkillLevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterSkillLevelUp>();
            var user = GetUser();
            var response = new ResCharacterSkillLevelUp();
            var character = user.Characters.FirstOrDefault(c => c.Csn == req.Csn);
            var charRecord = GameData.Instance.characterTable.Values.FirstOrDefault(c => c.id == character.Tid);
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
            var skillRecord = GameData.Instance.skillInfoTable.Values.FirstOrDefault(s => s.id == skillIdMap[req.Category] + (skillLevelMap[req.Category] - 1));
            var costRecord = GameData.Instance.costTable.Values.FirstOrDefault(c => c.id == skillRecord.level_up_cost_id);

            foreach (var cost in costRecord.costs.Where(i => i.item_type != "None"))
            {
                var item = user.Items.FirstOrDefault(i => i.ItemType == cost.item_id);

                item.Count -= cost.item_value;

                response.Items.Add(new NetUserItemData
                {
                    Isn = item.Isn,
                    Tid = cost.item_id,
                    Count = item.Count,
                    Csn = item.Csn,
                    Corporation = item.Corp,
                    Level = item.Level,
                    Exp = item.Exp,
                    Position = item.Position
                });
            }

            var newChar = new NetUserCharacterDefaultData
            {
                CostumeId = character.CostumeId,
                Csn = character.Csn,
                Level = character.Level,
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

            response.Character = newChar;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
