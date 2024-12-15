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

    public class TacticAcademyLessonReward
    {
        public int lesson_reward_id;
        public int lesson_reward_value;
    }
    public class TacticAcademyLessonRecord
    {
        public int currency_id;
        public int currency_value;
        public int id;
        public int group_id;
        public string lesson_type = "";
        public TacticAcademyLessonReward[]? lesson_reward;
    }
    
    public class TacticAcademyLessonTable
    {
        public List<TacticAcademyLessonRecord> records;
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
        public int piece_id;
		public string original_rare;
		public string corporation;
		public int grade_core_id;
		public int name_code;
		public int grow_grade;
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
    public class JukeboxListRecord
    {
        public int id;
        public int theme;
        public string bgm ;
        public bool is_loop;
        public int play_time;
        public string name ;
        public int order;
        public string artist;
        public string get_info_type;
        public string get_info_value;
    }

    public class JukeboxListTable
    {
        public List<JukeboxListRecord> records;
    }

    public class JukeboxThemeRecord
    {
        public int id;
        public string name_localkey;
        public string description_localkey;
        public int order;
        public string theme_resource;
        public string bg_color;
    }

    public class JukeboxThemeTable
    {
        public List<JukeboxThemeRecord> records;
    }
    public class OutpostBattleTableRecord
    {
        public int id;
        public int credit;
        public int character_exp1;
        public int character_exp2;
        public int user_exp;
    }
    public class OutpostBattleTable
    {
        public List<OutpostBattleTableRecord> records;
    }

    public class GachaPriceGroup
    {
        public int gacha_price_type;
        public int gacha_price_value_count_1;
        public int daily_gacha_discount_price_value_1;
        public int gacha_price_value_count_10;
    }

    public class GachaType
    {
        public int id;
        public string type;
        public int order_id;
        public int event_id;
        public string gacha_provide_count_type;
        public bool use_daily_discount_one;
        public int daily_free_gacha_event_id;
        public List<GachaPriceGroup> gacha_price_group;
        public int grade_prob_id;
        public bool is_max_count;
        public int max_ceiling_count;
        public int fixed_char_amount;
        public string gacha_page_prefab;
        public int pickup_char_group_id;
        public bool use_wish_list;
        public int gacha_play_max_count;
        public int gacha_reward_id;
        public int gacha_play_max_count_reward_id;
        public int previous_gacha_id;
    }

    public class GachaTypeTable
    {
        public List<GachaType> records;
    }
	
	public class EventManager
	{
		public int id;
		public string event_system_type;
		public string event_shortcut_id;
		public string name_localkey;
		public string description_localkey;
		public string schedule_type;
		public string schedule_value;
		public string event_disable_locale;
		public string event_resource_id;
		public string event_thumbnail_resource_table;
		public string event_thumbnail_resource_id;
		public string thumbnail_color;
		public string event_banner_resource_table;
		public string event_banner_resource_id;
		public long event_order;
		public bool is_popup;
		public string active_type;
		public string banner_print_type;
	}

	public class EventManagerTable
	{
		public List<EventManager> records;
	}
	
    public class LiveWallpaperRecord
    {
        public int id;
		public string livewallpaper_type;
    }

    public class LiveWallpaperTable
    {
        public List<LiveWallpaperRecord> records;
    }
	public class AlbumResourceRecord
	{
		public int id;
		public int sub_category_id;
		public string scenario_name_localtable = "";
		public string scenario_name_localkey = "";
		public string scenario_group_id = "";
		public int target_chapter;
		public bool is_hidden;
		public string dialogtype = "";
	}

	public class AlbumResourceTable
	{
		public List<AlbumResourceRecord> records;
	}
	
	public class UserFrameTableRecord
	{
		public int id;
		public string resource_id;
		public string tab_type;
		public string user_profile_type;
		public string filter_type;
		public int order;
		public string name_localkey;
		public string description_localkey;
		public bool is_sub_resource_prism;
	}

	public class UserFrameTable
	{
		public List<UserFrameTableRecord> records;
	}
	
	public class ArchiveRecordManagerRecord
	{
		public int id;
		public string record_type;
		public string record_title_locale;
		public int record_main_archive_event_id;
		public int record_list_order;
		public int unlock_ticket_id;
		public int unlock_ticket_count;
		public int reward_info_list_id;
		public int event_quest_clear_reward_id;
		public int recommended_story_list_id;
		public int included_contents_group_id;
		public string record_slot_bg_addressable;
		public string record_unlock_bg_addressable;
	}

	public class ArchiveRecordManagerTable
	{
		public List<ArchiveRecordManagerRecord> records;
	}
	
	public class ArchiveEventStoryRecord
	{
		public int id;
		public int event_id;
		public string prologue_scenario = "";
		public int dungeon_id;
		public int album_category_group;
		public string ui_prefab = "";
		public int archive_ticket_item_id;
		public int archive_currency_item_id;
	}

	public class ArchiveEventStoryTable
	{
		public List<ArchiveEventStoryRecord> records;
	}

	public class ArchiveEventQuestRecord
	{
		public int id;
		public int event_quest_manager_id;
		public string condition_type = "";
		public int condition_value;
		public string name_localkey = "";
		public string description_localkey = "";
		public int next_quest_id;
		public string end_scenario_id = "";
	}

	public class ArchiveEventQuestTable
	{
		public List<ArchiveEventQuestRecord> records;
	}
	
	public class ArchiveEventDungeonStageRecord
	{
		public int id;
		public int group;
		public int step;
		public string stage_name = "";
		public string stage_contents_type = "";
		public int stage_id;
		public bool is_repeat_clear;
	}

	public class ArchiveEventDungeonStageTable
	{
		public List<ArchiveEventDungeonStageRecord> records;
	}
    public class UserTitleRecord
    {
        public int id;
        public int order;
        public string user_title_production_type = "";
        public int user_title_production_id;
        public string icon_resource_id = "";
        public string name_locale_key = "";
        public string desc_locale_key = "";
        public int reward_id;
        public bool not_acquired_is_visible;
    }

    public class UserTitleTable
    {
        public List<UserTitleRecord> records;
    }
	
    public class ArchiveMessengerConditionList
    {
        public string condition_type;
        public int condition_id;
    }

    public class ArchiveMessengerConditionRecord
    {
        public int id;
        public int archive_messenger_group_id;
        public List<ArchiveMessengerConditionList> archive_messenger_condition_list;
        public string tid;
    }

    public class ArchiveMessengerConditionTable
    {
        public string version;
        public List<ArchiveMessengerConditionRecord> records;
    }

}
