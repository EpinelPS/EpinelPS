namespace EpinelPS.StaticInfo
{
    public class MainQuestCompletionRecord
    {
        public int id;
        public int group_id;
        public string category = "";
        public int condition_id;
        public int next_main_quest_id = 0;
        public int reward_id = 0;
        public int target_chapter_id;
    }
    public class MainQuestCompletionTable
    {
        public List<MainQuestCompletionRecord> records;
    }
    public class CampaignStageRecord
    {
        public int id;
        public int chapter_id;
        public string stage_category = "";
        public int reward_id = 0;
        /// <summary>
        /// Can be Normal or Hard
        /// </summary>
        public string chapter_mod = "";
        public string stage_type = "";
        public string enter_scenario = "";
        public string exit_scenario = "";
    }
    public class CampaignStageTable
    {
        public List<CampaignStageRecord> records;
    }
    public class RewardTableRecord
    {
        public int id;
        public int user_exp;
        public int character_exp;
        public RewardEntry[]? rewards;
    }
    public class RewardTable
    {
        public List<RewardTableRecord> records;
    }

    public class RewardEntry
    {
        /// <summary>
        /// example: 1000000
        /// </summary>
        public int reward_percent;
        public string percent_display_type = "";
        public string reward_type = "";
        public int reward_id;
        public int reward_value;
    }


    public class ClearedTutorialData
    {
        public int id;
        public int VersionGroup = 0;
        public int GroupId;
        public int ClearedStageId;
        public int NextId;
        public bool SaveTutorial;
    }
    public class TutorialTable
    {
        public List<ClearedTutorialData> records;
    }

    public class CharacterLevelData
    {
        /// <summary>
        /// level
        /// </summary>
        public int level;
        /// <summary>
        /// can be CharacterLevel or SynchroLevel
        /// </summary>
        public string type = "";
        /// <summary>
        /// amount of credits required
        /// </summary>
        public int gold = 0;
        /// <summary>
        /// amount of battle data required
        /// </summary>
        public int character_exp = 0;
        /// <summary>
        /// amount of core dust required
        /// </summary>
        public int character_exp2 = 0;
    }

    public class TacticAcademyLessonRecord
    {
        public CurrencyType CurrencyId;
        public int CurrencyValue;
        public int Id;
        public int GroupId;
    }

    public class CampaignChapterRecord
    {
        public int id;
        public int chapter;
        public string field_id;
        public string hard_field_id;
    }
    public class CampaignChapterTable
    {
        public List<CampaignChapterRecord> records;
    }

    public class CharacterRecord
    {
        public int id;
        // TODO: There is more stuff here but it isn't needed yet
    }
    public class CharacterTable
    {
        public List<CharacterRecord> records;
    }

    public class ItemEquipRecord
    {
        public int id;
        public string item_sub_type;
    }
    public class ItemEquipTable
    {
        public List<ItemEquipRecord> records;
    }

    public class FieldItemRecord
    {
        public int id;
        public string item_type;
        public int type_value;
        public bool is_final_reward;
        public string difficulty;
    }
    public class FieldItemTable
    {
        public List<FieldItemRecord> records;
    }
}
