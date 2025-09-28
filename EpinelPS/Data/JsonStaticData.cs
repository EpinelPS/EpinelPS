using MemoryPack;
using System.Data;
using System.Runtime.Serialization;

namespace EpinelPS.Data
{
    public class DataTable<T>
    {
        public string version { get; set; } = "";
        public List<T> records { get; set; } = [];
    }
    public enum Category
    {
        Campaign_View,
        Outpost_View,
        Ark_View,
        Lobby_Enter,
        Campaign_Enter,
        Outpost_Building_Enter,
        Ark_Marker,
        Campaign_Clear,
        Outpost_Select,
        Tribe_Tower_Clear,
        Tribe_Tower_Enter,
        Tribe_Tower_View,
        Run_Gacha,
        Npc_Talk,
        FieldObject_Collection,
        End,
        LostSector_View,
        LostSector_Clear,
        SimulationRoom_View,
        EventQuest_Stage_Enter,
        EventQuest_Stage_Clear,
        EventQuest_Stage_Group_Clear,
        EventQuest_Popup_Enter,
        Normal_Chapter_View,
        Field_Interaction_Action_Trigger,
        FavoriteItemQuest_Stage_Enter,
        FavoriteItemQuest_Stage_Clear,
        FavoriteItemQuest_Stage_Group_Clear,
        SimulationRoom_Select
    }
    public enum ScenarioType
    {
        None,
        FieldTalk,
        Dialog,
        Balloon
    }
    public enum ChapterMod
    {
        Normal,
        Hard
    }
    public enum StageCategory
    {
        Normal,
        Story,
        Hard,
        Extra,
        Boss
    }
    public enum StageType
    {
        None,
        Main,
        Sub,
        Emergency,
        EventQuest,
        FavoriteItemQuest
    }
    public enum EquipmentRarityType
    {
        None,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10
    }
    [MemoryPackable]
    public partial class MainQuestCompletionRecord
    {
        public int id;
        public int group_id;
        public Category category;
        public int condition_id;
        public string condition_ui_localkey = "";
        public string shortcut_type = "";
        public int shortcut_value;
        public string name_localkey = "";
        public string description_localkey = "";
        public int next_main_quest_id;
        public int reward_id;
        public ScenarioType scenario_type;
        public string episode_id = "";
        public int target_chapter_id;
        public string header_bg_resource_id = "";
    }
    [MemoryPackable]
    public partial class CampaignStageRecord
    {
        public int id;
        public int chapter_id;
        public ChapterMod chapter_mod;
        public int parent_id;
        public int group_id;
        public string name_localkey = "";
        public StageCategory stage_category;
        public StageType stage_type;
        public bool spot_autocontrol;
        public int enter_condition;
        public int monster_stage_lv;
        public int dynamic_object_stage_lv;
        public int standard_battle_power;
        public int stage_stat_increase_group_id;
        public bool is_use_quick_battle;
        public int field_monster_id;
        public int spot_id;
        public int reward_id;
        public ScenarioType enter_scenario_type;
        public string enter_scenario = "";
        public ScenarioType exit_scenario_type;
        public string exit_scenario = "";
        public int current_outpost_battle_id;
        public int cleared_battleid;
        public int fixedCharacterid;
        public int characterLevel;
    }
    [MemoryPackable]
    public partial class RewardRecord
    {
        public int id;
        public int user_exp;
        public int character_exp;
        public List<RewardEntry> rewards = [];
    }

    public enum PercentDisplayType
    {
        None,
        Percent,
        Random
    }
    public enum RewardType
    {
        None = 0,
        User_exp = 1,
        Char_exp = 2,
        Currency = 3,
        Character = 4,
        Item = 5,
        Frame = 6,
        AttractivePoint = 7,
        Bgm = 8,
        Point = 9,
        LiveWallpaper = 10,
        Memorial = 11,
        CharacterCostume = 12,
        ItemRandom = 13,
        InfraCoreExp = 14,
        ItemRandomBox = 15,
        Equipment_None = 16,
        Equipment_MISSILIS = 17,
        Equipment_ELYSION = 18,
        Equipment_TETRA = 19,
        Equipment_PILGRIM = 20,
        Equipment_Random_01 = 21,
        Equipment_Random_02 = 22,
        Equipment_Random_03 = 23,
        PassPoint = 41,
        Equipment_ABNORMAL = 42,
        FavoriteItem = 43,
        ProfileCardObject = 44,
        ProfileRandomBox = 45,
        UserTitle = 46,
        LobbyDecoBackground = 47
    }


    [MemoryPackable]
    public partial class RewardEntry
    {
        /// <summary>
        /// example: 1000000
        /// </summary>
        public int reward_percent;
        public PercentDisplayType percent_display_type;
        public RewardType reward_type;
        public int reward_id;
        public int reward_value;
    }
    public class ClearedTutorialData
    {
        public int id;
        public int VersionGroup;
        public int GroupId;
        public int ClearedStageId;
        public int NextId;
        public bool SaveTutorial;
    }
    [MemoryPackable]
    public partial class CharacterLevelData
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
        public int gold;
        /// <summary>
        /// amount of battle data required
        /// </summary>
        public int character_exp;
        /// <summary>
        /// amount of core dust required
        /// </summary>
        public int character_exp2;
    }
    public class TacticAcademyLessonReward
    {
        public int lesson_reward_id;
        public int lesson_reward_value;
    }
    [MemoryPackable]
    public partial class TacticAcademyLessonRecord
    {
        public int currency_id;
        public int currency_value;
        public int id;
        public int group_id;
        public string lesson_type = "";
        public TacticAcademyLessonReward[]? lesson_reward;
    }
    public enum OpenType
    {
        Open,
        NonUpdate
    }

    public enum ChapterGuideType
    {
        None,
        Tutorial,
        Common
    }


    [MemoryPackable]
    public partial class CampaignChapterRecord
    {
        public int id;
        public string name_localized = "";
        public string description_localized = "";
        public int world;
        public int chapter;
        public OpenType openType;
        public DateTime openTime;
        public ChapterGuideType chapter_guide;
        public string chapter_image = "";
        public string hard_chapter_image = "";
        public string field_id = "";
        public string hard_field_id = "";
    }
    [MemoryPackable]
    public partial class CharacterRecord
    {
        public int id;
        public int piece_id;
        public string original_rare = "";
        public string corporation = "";
        public string corporation_sub_type = "";
        public int grade_core_id;
        public int name_code;
        public int grow_grade;
        public int stat_enhance_id;
        public string character_class = "";
        public List<int> element_id = [];
        public int critical_ratio;
        public int critical_damage;
        public int shot_id;
        public int bonusrange_min;
        public int bonusrange_max;
        public string use_burst_skill = "";
        public string change_burst_step = "";
        public int burst_apply_delay;
        public int burst_duration;
        public int ulti_skill_id;
        public int skill1_id;
        public string skill1_table = "";
        public int skill2_id;
        public string skill2_table = "";
        public string eff_category_type = "";
        public int eff_category_value;
        public string category_type_1 = "";
        public string category_type_2 = "";
        public string category_type_3 = "";
        public string cv_localkey = "";
        public string squad = "";
        public bool is_visible;
        public bool prism_is_active;
        public bool is_detail_close;
    }

    [MemoryPackable]
    public partial class ItemEquipRecord
    {
        public int id;
        public string name_localkey = "";
        public string description_localkey = "";
        public string resource_id = "";
        public string item_type = "";
        public string item_sub_type = "";
        public string @class = "";
        public string item_rare = "";
        public int grade_core_id;
        public int grow_grade;
        public List<Stat> stat = null!;
        public List<OptionSlot> option_slot = null!;
        public int option_cost;
        public int option_change_cost;
        public int option_lock_cost;
    }

    public class Stat
    {
        public string stat_type = "";
        public int stat_value;
    }

    public class OptionSlot
    {
        public int option_slot;
        public int option_slot_success_ratio;
    }

    [MemoryPackable]
    public partial class FieldItemRecord
    {
        public int id;
        public string item_type = "";
        public int type_value;
        public bool is_final_reward;
        public string difficulty = "";
    }

    [MemoryPackable]
    public partial class JukeboxListRecord
    {
        public int id;
        public int theme;
        public string bgm = "";
        public bool is_loop;
        public int play_time;
        public string name = "";
        public int order;
        public string artist = "";
        public string get_info_type = "";
        public string get_info_value = "";
    }

    [MemoryPackable]
    public partial class JukeboxThemeRecord
    {
        public int id;
        public string name_localkey = "";
        public string description_localkey = "";
        public int order;
        public string theme_resource = "";
        public string bg_color = "";
    }

    public enum PrepareRewardType
    {
        None,
        Currency,
        ItemRandom
    }

    [MemoryPackable]
    public partial class OutpostBattle_Data
    {
        public int view_item_id;
        public PrepareRewardType type;
        public int item_id;
        public int item_value;
        public int item_sec;
    }

    [MemoryPackable]
    public partial class OutpostBattleTableRecord
    {
        public int id;
        public int battle_box_level;
        public int main_stage_clear_count;
        public int reward_id;
        public int credit;
        public int time_credit;
        public int character_exp1;
        public int time_charexp1;
        public int user_exp;
        public int time_user_exp;
        public int character_exp2;
        public int time_charexp2;
        public List<OutpostBattle_Data> RewardList = [];
    }

    [MemoryPackable]
    public partial class GachaPriceGroup
    {
        public int gacha_price_type;
        public int gacha_price_value_count_1;
        public int daily_gacha_discount_price_value_1;
        public int gacha_price_value_count_10;
    }

    public partial class GachaType
    {
        public int id;
        public string type = "";
        public int order_id;
        public int event_id;
        public string gacha_provide_count_type = "";
        public bool use_daily_discount_one;
        public int daily_free_gacha_event_id;
        public List<GachaPriceGroup> gacha_price_group = [];
        public int grade_prob_id;
        public bool is_max_count;
        public int max_ceiling_count;
        public int fixed_char_amount;
        public string gacha_page_prefab = "";
        public int pickup_char_group_id;
        public bool use_wish_list;
        public int gacha_play_max_count;
        public int gacha_reward_id;
        public int gacha_play_max_count_reward_id;
        public int previous_gacha_id;
    }

    [MemoryPackable]
    public partial class GachaGradeProbRecord
    {
        public int id;
        public int group_id;
        public string rare = "";
        public int prob;
        public int gacha_list_id;
    }
    [MemoryPackable]
    public partial class GachaListProbRecord
    {
        public int id;
        public int group_id;
        public int gacha_id;
    }

    [MemoryPackable]
    public partial class EventManager
    {
        public int id;
        public string event_system_type = "";
        public string event_shortcut_id = "";
        public string name_localkey = "";
        public string description_localkey = "";
        public string schedule_type = "";
        public string schedule_value = "";
        public string event_disable_locale = "";
        public string event_resource_id = "";
        public string event_thumbnail_resource_table = "";
        public string event_thumbnail_resource_id = "";
        public string thumbnail_color = "";
        public string event_banner_resource_table = "";
        public string event_banner_resource_id = "";
        public long event_order;
        public bool is_popup;
        public string active_type = "";
        public string banner_print_type = "";
    }
    [MemoryPackable]
    

    public partial class LiveWallpaperRecord
    {
        public int id;
        public string livewallpaper_type = "";
    }
    [MemoryPackable]
    
    public partial class AlbumResourceRecord
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


    [MemoryPackable]
    public partial class UserFrameTableRecord
    {
        public int id;
        public string resource_id = "";
        public string tab_type = "";
        public string user_profile_type = "";
        public string filter_type = "";
        public int order;
        public string name_localkey = "";
        public string description_localkey = "";
        public bool is_sub_resource_prism;
    }


    [MemoryPackable]
    public partial class ArchiveRecordManagerRecord
    {
        public int id;
        public string record_type = "";
        public string record_title_locale = "";
        public int record_main_archive_event_id;
        public int record_list_order;
        public int unlock_ticket_id;
        public int unlock_ticket_count;
        public int reward_info_list_id;
        public int event_quest_clear_reward_id;
        public int recommended_story_list_id;
        public int included_contents_group_id;
        public string record_slot_bg_addressable = "";
        public string record_unlock_bg_addressable = "";
    }


    [MemoryPackable]
    public partial class ArchiveEventStoryRecord
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


    [MemoryPackable]
    public partial class ArchiveEventQuestRecord
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


    [MemoryPackable]
    public partial class ArchiveEventDungeonStageRecord
    {
        public int id;
        public int group;
        public int step;
        public string stage_name = "";
        public string stage_contents_type = "";
        public int stage_id;
        public bool is_repeat_clear;
    }

    [MemoryPackable]
    public partial class UserTitleRecord
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


    [MemoryPackable]
    public partial class ArchiveMessengerConditionList
    {
        public string condition_type = "";
        public int condition_id;
    }
    [MemoryPackable]
    public partial class ArchiveMessengerConditionRecord
    {
        public int id;
        public int archive_messenger_group_id;
        public List<ArchiveMessengerConditionList> archive_messenger_condition_list = [];
        public string tid = "";
    }
    [MemoryPackable]
    public partial class CharacterStatRecord
    {
        public int id;
        public int group;
        public int level;
        public int level_hp;
        public int level_attack;
        public int level_defence;
        public int level_energy_resist;
        public int level_bio_resist;
    }

    [MemoryPackable]
    public partial class ItemMaterialRecord
    {
        public int id;
        public string name_localkey = "";
        public string description_localkey = "";
        public string resource_id = "";
        public string item_type = "";
        public string item_sub_type = "";
        public string item_rare = "";
        public int item_value;
        public string material_type = "";
        public int material_value;
        public int stack_max;
    }


    [MemoryPackable]
    public partial class SkillInfoRecord
    {
        public int id;
        public int group_id;
        public int skill_level;
        public int next_level_id;
        public int level_up_cost_id;
        public string icon = "";
        public string name_localkey = "";
        public string description_localkey = "";
        public string info_description_localkey = "";
        public List<DescriptionValue> description_value_list = [];
    }

    public class DescriptionValue
    {
        public string description_value = "";
    }

    public class CostRecord
    {
        public int id;
        public List<CostData> costs = [];
    }

    public class CostData
    {
        public string item_type = "";
        public int item_id;
        public int item_value;
    }

    [MemoryPackable]
    public partial class MidasProductRecord
    {
        public int id;
        public string product_type = "";
        public int product_id;
        public string item_type = "";
        public string midas_product_id_proximabeta = "";
        public string midas_product_id_gamamobi = "";
        public bool is_free;
        public string cost = "";
    }
    
    public enum ShopCategoryType
    {
        None = 0,
        Normal = 1,
        Guild = 2,
        Disassemble = 3,
        Maze = 4,
        PvP = 5,
        StoryEvent = 6,
        Mileage = 7,
        Trade = 8
    }
    [MemoryPackable]
    public partial class TowerRecord
    {
        public int id;
        public int floor;
        public string type = "";
        public int standard_battle_power;
        public int reward_id;
    }

    [MemoryPackable]
    public partial class ItemEquipExpRecord
    {
        public int id;
        public int level;
        public int exp;
        public string item_rare = "";
        public int grade_core_id;
    }


    [MemoryPackable]
    public partial class ItemEquipGradeExpRecord
    {
        public int id;
        public EquipmentRarityType item_rare;
        public int grade_core_id;
        public int exp;
    }
    [MemoryPackable]
    public partial class UserExpRecord
    {
        public int level;
        public int exp;
        public int reward_id;
    }

    [MemoryPackable]
    public partial class CharacterCostumeRecord
    {
        public int id;
    }

    [MemoryPackable]
    public partial class SideStoryStageRecord
    {
        public int id;
        public int first_clear_reward;
    }


    [MemoryPackable]
    public partial class TriggerRecord
    {
        public int id;
        public int condition_id;
        public int condition_value;
        public int reward_id;
        public int point_value;
        public bool print_value;
        public int before_trigger_id;
    }
    
    public class InfracoreFunction
    {
        public int function;
    }
    [MemoryPackable]
    public partial class InfracoreRecord
    {
        public int id;
        public int grade;
        public int reward_id;
        public int infra_core_exp;
        public List<InfracoreFunction> function_list = [];
    }
    [MemoryPackable]
    public partial class AttractiveCounselCharacterRecord
    {
        public int id;
        public int name_code;
        public int collect_reward_id;
    }

    [MemoryPackable]
    public partial class AttractiveLevelRewardRecord
    {
        public int id;
        public int name_code;
        public int reward_id;
        public int attractive_level;
        public int costume;
    }

    [MemoryPackable]
    public partial class AttractiveLevelRecord
    {
        public int id;
        public int attractive_level;
        public int attractive_point;
        public int attacker_hp_rate;
        public int attacker_attack_rate;
        public int attacker_defence_rate;
        public int attacker_energy_resist_rate;
        public int attacker_metal_resist_rate;
        public int attacker_bio_resist_rate;
        public int defender_hp_rate;
        public int defender_attack_rate;
        public int defender_defence_rate;
        public int defender_energy_resist_rate;
        public int defender_metal_resist_rate;
        public int defender_bio_resist_rate;
        public int supporter_hp_rate;
        public int supporter_attack_rate;
        public int supporter_defence_rate;
        public int supporter_energy_resist_rate;
        public int supporter_metal_resist_rate;
        public int supporter_bio_resist_rate;
    }
    [MemoryPackable]
    public partial class SubquestRecord
    {
        public int id;
        public int group_id;
        public int clear_condition_id;
        public int clear_condition_value;
        public string conversation_id = "";
        public string end_messenger_conversation_id = "";
        public int before_sub_quest_id;
    }

    [MemoryPackable]
    public partial class MessengerDialogRecord
    {
        public string id = "";
        public string conversation_id = "";
        public string jump_target = "";
        public bool is_opener;
        public int reward_id;
    }
    [MemoryPackable]
    public partial class MessengerMsgConditionRecord
    {
        public int id;
        public List<MessengerConditionTriggerList> trigger_list = [];
        public string message_type = "";
        public string tid = "";
        public int resource_id;
        public int stamina_value;
        public int reward_id;
    }

    [MemoryPackable]
    public partial class MessengerConditionTriggerList
    {
        public string trigger = "";
        public int condition_id;
        public int condition_value;
    }


    public enum ScenarioRewardCondition
    {
        MainScenario,
        AttractiveScenario
    }

    [MemoryPackable]
    public partial class ScenarioRewardRecord
    {
        public int id;
        public ScenarioRewardCondition condition_type;
        public string condition_id = "";
        public int reward_id;
    }

    [MemoryPackable]
    public partial class ProductOfferRecord
    {
        public int id;
    }

    [MemoryPackable]
    public partial class InterceptionRecord
    {
        public int id;
        public int group;
        public int condition_reward_group;
        public int percent_condition_reward_group;
        public long fixed_damage;
    }

    [MemoryPackable]
    public partial class ConditionRewardRecord
    {
        public int id;
        public int group;
        public int priority;
        public long value_min;
        public long value_max;
        public int reward_id;
    }
    
    public enum ItemSubType
    {
        BundleBox,
        ItemRandomBoxList,
        ItemRandomBoxNormal,
        TimeReward,
        Box,
        ProfileRandomBox,
        ArcadeItem,
        EquipCombination
    }
    [MemoryPackable]
    public partial class ItemConsumeRecord
    {
        public int id;
        public string use_type = "";
        public string item_type = "";
        public ItemSubType item_sub_type;
        public int use_id;
        public int use_value;
    }

    [MemoryPackable]
    public partial class ItemPieceRecord
    {
        public int id;
        public string use_type = "";
        public int use_id;
        public int use_value;
    }

    [MemoryPackable]
    public partial class RandomItemRecord
    {
        public int id;
        public int group_id;
        public RewardType reward_type;
        public int reward_id;
        public int reward_value_min;
        public int reward_value_max;
        public int ratio;
    }

    [MemoryPackable]
    public partial class RecycleResearchStatRecord
    {
        public int id;
        public string recycle_type = "";
        public int unlock_condition_id;
        public int unlock_level;
        public int attack;
        public int defense;
        public int hp;
    }

    [MemoryPackable]
    public partial class RecycleResearchLevelRecord
    {
        public int id;
        public string recycle_type = "";
        public int recycle_level;
        public int exp;
        public int item_id;
        public int item_value;
    }
    
    public enum ContentOpenType
    {
        Stage,
        NonUpdate
    }

    [MemoryPackable]
    public partial class LostSectorRecord
    {
        public int id;
        public int sector;
        public int exploration_reward;
        public string field_id { get; set; } = "";
        public int sector_clear_condition;
        public ContentOpenType open_condition_type;
        public int open_condition_value;

    }
    [MemoryPackable]
    public partial class LostSectorStageRecord
    {
        public int id;
        public bool is_use_quick_battle;
        public int sector;
    }
    

    public class ItemSpawner
    {
        public string positionId = "";
        public int itemId;
        public bool isCompleteReward;
    }
    public class StageSpawner
    {
        public string positionId = "";
        public int stageId;
    }

    [MemoryPackable]
    public partial class MapInfo
    {
        public string id { get; set; } = "";
        public List<ItemSpawner> ItemSpawner { get; set; } = [];
        public List<StageSpawner> StageSpawner { get; set; } = [];
    }

    // Harmony Cube  Data Structures
    [MemoryPackable]
    public partial class ItemHarmonyCubeRecord
    {
        public int id;
        public string name_localkey = "";
        public string description_localkey = "";
        public int location_id;
        public string location_localkey = "";
        public int order;
        public int resource_id;
        public string bg = "";
        public string bg_color = "";
        public string item_type = "";
        public string item_sub_type = "";
        public string item_rare = "";
        public string @class = "";
        public int level_enhance_id;
        public List<HarmonyCubeSkillGroup> harmonycube_skill_group = [];
    }

    [MemoryPackable]
    public partial class ItemHarmonyCubeLevelRecord
    {
        public int id;
        public int level_enhance_id;
        public int level;
        public List<HarmonyCubeSkillLevel> skill_levels = [];
        public int material_id;
        public int material_value;
        public int gold_value;
        public int slot;
        public List<HarmonyCubeStat> harmonycube_stats = [];
    }

    public class HarmonyCubeSkillGroup
    {
        public int skill_group_id;
    }

    public class HarmonyCubeSkillLevel
    {
        public int skill_level;
    }

    public class HarmonyCubeStat
    {
        public string stat_type = "";
        public int stat_rate;
    }

    [MemoryPackable]
    public partial class FavoriteItemRecord
    {
        public int id;
        public string name_localkey = "";
        public string description_localkey = "";
        public string icon_resource_id = "";
        public string img_resource_id = "";
        public string prop_resource_id = "";
        public int order;
        public string favorite_rare = "";
        public string favorite_type = "";
        public string weapon_type = "";
        public int name_code;
        public int max_level;
        public int level_enhance_id;
        public int probability_group;
        public int albumcategory_id;
    }

    [MemoryPackable]
    public partial class FavoriteItemExpRecord
    {
        public int id;
        public string favorite_rare = "";
        public int level;
        public int need_exp;
    }

    [MemoryPackable]
    public partial class FavoriteItemLevelRecord
    {
        public int id;
        public int level_enhance_id;
        public int grade;
        public int level;
        public List<FavoriteItemStatData> favoriteitem_stat_data = [];
        public List<CollectionSkillLevelData> collection_skill_level_data = [];
    }

    [MemoryPackable]
    public partial class FavoriteItemProbabilityRecord
    {
        public int id;
        public int probability_group;
        public int level_min;
        public int level_max;
        public int need_item_id;
        public int need_item_count;
        public int exp;
        public int great_success_rate;
        public int great_success_level;
    }

    [MemoryPackable]
    public partial class FavoriteItemQuestRecord
    {
        public int id;
        public int name_code;
        public string condition_type = "";
        public int condition_value;
        public string quest_thumbnail_resource_id = "";
        public string name_localkey = "";
        public string description_localkey = "";
        public int next_quest_id;
        public string end_scenario_id = "";
        public int reward_id;
    }

    public class FavoriteItemStatData
    {
        public string stat_type = "";
        public int stat_value;
    }

    public class CollectionSkillLevelData
    {
        public int collection_skill_level;
    }

    [MemoryPackable]
    public partial class FavoriteItemQuestStageRecord
    {
        public int id;
        public int group_id;
        public int chapter_id;
        public string chapter_mod = "";
        public int name_code;
        public int spawn_condition_favoriteitem_quest_id;
        public int spawn_condition_campaign_stage_id;
        public int enter_condition_favoriteitem_quest_id;
        public int enter_condition_campaign_stage_id;
        public string name_localkey = "";
        public string stage_category = "";
        public bool spot_autocontrol;
        public int monster_stage_lv;
        public int dynamic_object_stage_lv;
        public int standard_battle_power;
        public int stage_stat_increase_group_id;
        public bool is_use_quick_battle;
        public int field_monster_id;
        public int spot_id;
        public int state_effect_function_id;
        public int reward_id;
        public string enter_scenario_type = "";
        public string enter_scenario = "";
        public int fixed_play_character_id;
        public int spawn_condition_favoriteitem_quest_stage_id;
        public int enter_condition_favoriteitem_quest_stage_id;
    }

    [MemoryPackable]
    public partial class EventPlaySodaManagerRecord
    {
        public int id;
        public int event_id;
        public int daily_reward_id;
        public int challenge_group_id;
        public int play_helptip_id;
        public int story_group_id;
        public int total_point_reward_group_id;
        public int ingame_pause_second;
        public MiniGameSystemType minigame_type;
        public string ui_prefab_keyword = "";
    }

    public enum MiniGameSystemType
    {
        Normal,
        Archive,
        Arcade
    }

    [MemoryPackable]
    public partial class EventPlaySodaStoryModeRecord
    {
        public int id;
        public int group_id;
        public int stage_data_id;
        public int stage_helptip_id;
        public string stage_image = "";
        public int order;
        public EventDungeonOpenCondition open_condition_type;
        public int open_condition_value;
        public string game_title = "";
        public EventPlaySodaGameType game_type;
        public string short_name = "";
        public int clear_reward_id;
        public string enter_scenario = "";
        public string exit_scenario = "";
    }

    [MemoryPackable]
    public partial class EventPlaySodaChallengeModeRecord
    {
        public int id;
        public int group_id;
        public int stage_data_id;
        public int stage_helptip_id;
        public string stage_image = "";
        public int order;
        public EventDungeonOpenCondition open_condition_type;
        public int open_condition_value;
        public string game_title = "";
        public string game_title_en = "";
        public EventPlaySodaGameType game_type;
        public string game_mode_resource = "";
    }

    public enum EventPlaySodaGameType
    {
        Running = 2001,
        Smash = 2002,
        CatchCoin = 2003,
        StackUp = 2004,
        Climbing = 2005
    }

    public enum EventDungeonOpenCondition
    {
        None,
        EventDungeonStageClear,
        EventDay
    }

    [MemoryPackable]
    public partial class EventPlaySodaPointRewardRecord
    {
        public int id;
        public int group_id;
        public int point_value;
        public EventPlaySodaGameType game_type;
        public int step;
        public int reward_id;
    }

    [MemoryPackable]
    public partial class EventMVGQuestRecord_Raw
    {
        public int id;
        public int condition_id;
        public int next_quest_id;
        public int parents_quest_id;
        public int time_line_group_id;
        public int reward_id;
        public EventMVGQuestConditionType condition_type;
        public int condition_value_1;
        public string condition_value_2 = "";
        public string description_localkey = "";
        public string name_localkey = "";
        public MVGQuestTargetType quest_target_type;
        public MVGQuestType quest_type;
    }

    public enum EventMVGQuestConditionType
    {
        StartMission,
        EndMission,
        KillMonster,
        MissionTargetPoint,
        ClearMultiCondition,
        GetCollectable,
        GetCurrency,
        PlatformSwitchInteraction,
        ConditionCheckSwitchInteraction,
        TeleportDeviceInteraction
    }

    public enum MVGQuestTargetType
    {
        First,
        Loop
    }

    public enum MVGQuestType
    {
        Normal,
        MotherMission,
        ChildMission
    }

    [MemoryPackable]
    public partial class EventMVGShopRecord_Raw
    {
        public int id;
        public int group_id;
        public int itemId;
        public string desc = "";
        public int itemSlot;
        public EventMVGShopItemTypeData itemType;
        public EventMVGQuestConditionType condition_type;
        public int order;
        //public List<  condition_value_2 = "";
        public string description_localkey = "";
        public string name_localkey = "";
        public MVGQuestTargetType quest_target_type;
        public MVGQuestType quest_type;
    }

    public enum EventMVGShopItemTypeData
    {
        Collectable,
        Module,
        None
    }

    [MemoryPackable]
    public partial class EventMVGMissionRecord_Raw
    {
        public int id;
        public int mvg_id;
        public int condition_id;
        public int condition_value;
        public int order;
        public string name_localkey = "";
        public EventMVGMissionType mission_type;
        public EventMVGMIssionSubType mission_sub_type;
        public EventMVGMissionConditionType condition_type;
        public bool display_type;
        public bool is_reset;
        public int reward_id;
        public int point_value;
    }

    public enum EventMVGMissionType
    {
        Daily,
        Achievement
    }

    public enum EventMVGMIssionSubType
    {
        None,
        MonsterKill,
        BossMonsterKill,
        GetItem,
        ChapterClear,
        Explorer
    }

    public enum EventMVGMissionConditionType
    {
        KillMonster,
        KillBossMonster,
        GetCurrency,
        GetCollectable,
        None,
        ClearAchievement,
        TargetPoint,
        ClearMap,
        GetUpgradeMax,
        GetModule,
        UseKey,
        GetMemory,
        PlayMVG,
    }
}
