using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.StaticInfo
{
    public class MainQuestCompletionData
    {
        public int id;
        public int group_id;
        public string category = "";
        public int condition_id;
        public int next_main_quest_id = 0;
        public int reward_id = 0;
        public int target_chapter_id;
    }
    public class CampaignStageRecord
    {
        public int id;
        public int chapter_id;
        public string stage_category = "";
        public int reward_id = 0;
    }
    public class RewardTableRecord
    {
        public int id;
        public int user_exp;
        public int character_exp;
        public RewardEntry[]? rewards;
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
    
}
