using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    public static class ClearEventStageHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClearEventStageHelper));

        /// <summary>
        /// Clear event stage and get rewards  
        /// </summary>
        /// <param name="user">The user clearing the stage</param>
        /// <param name="stageId">The ID of the stage being cleared</param>
        /// <param name="reward">The reward data for the stage</param>
        /// <param name="bonusReward">The bonus reward data for the stage</param>
        /// <param name="battleResult">The result of the battle</param>
        /// <param name="clearCount">The number of times the stage has been cleared</param>
        public static void ClearStage(User user, int stageId, ref NetRewardData reward, ref NetRewardData bonusReward, int battleResult = 0, int clearCount = 0)
        {
            if (battleResult != 1) return;
            if (clearCount < 1) clearCount = 1;
            GetReward(user, stageId, ref reward, clearCount);
            GetBonusReward(user, stageId, ref bonusReward, clearCount);
        }

        /// <summary>
        /// Get normal reward for clearing event stage
        /// </summary>
        /// <param name="user">The user clearing the stage</param>
        /// <param name="stageId">The ID of the stage being cleared</param>
        /// <param name="reward">The reward data for the stage</param>
        /// <param name="battleResult">The result of the battle</param>
        /// <param name="clearCount">The number of times the stage has been cleared</param>
        public static void GetReward(User user, int stageId, ref NetRewardData reward, int clearCount)
        {
            int rewardId = GetRewardId(stageId);
            if (rewardId == 0) return;
            RecievedReward(user, ref reward, rewardId, clearCount);
        }

        /// <summary>
        /// Get bonus reward for clearing event stage   
        /// </summary>
        /// <param name="user">The user clearing the stage </param>
        /// <param name="stageId">The ID of the stage being cleared</param>
        /// <param name="bonusReward">The bonus reward data for the stage</param>
        /// <param name="battleResult">The result of the battle</param>
        /// <param name="clearCount">The number of times the stage has been cleared</param>
        public static void GetBonusReward(User user, int stageId, ref NetRewardData bonusReward, int clearCount)
        {
            int rewardId = GetBonusRewardId(stageId);
            if (rewardId == 0) return;
            RecievedReward(user, ref bonusReward, rewardId, clearCount);
        }

        private static void RecievedReward(User user, ref NetRewardData reward,int rewardId, int clearCount)
        {
            RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(rewardId);
            if (rewardData == null)
            {
                Logging.WriteLine($"unknown reward Id {rewardId}", LogType.Error);
                return;
            }
            foreach (var item in rewardData.Rewards)
            {
                if (item == null) continue;
                if (item.RewardType == RewardType.None) continue;
                RewardUtils.AddSingleObject(user, ref reward, item.RewardId, item.RewardType, item.RewardValue * clearCount);
            }
        }

        /// <summary>
        /// Get reward Id from EventDungeonSpotBattleTable
        /// </summary>
        /// <param name="stageId">The ID of the stage being cleared</param>
        /// <returns>The reward ID for the stage</returns>
        private static int GetRewardId(int stageId)
        {
            if (GameData.Instance.EventDungeonSpotBattleTable.TryGetValue(stageId, out EventDungeonSpotBattleRecord? stageRecord))
            {
                return stageRecord.ClearRewardId;
            }
            return 0;
        }

        /// <summary>
        /// Get bonus reward Id from EventDungeonTable via EventDungeonStageTable and EventDungeonDifficultTable
        /// </summary>    
        /// <param name="stageId">The ID of the stage being cleared</param>
        /// <returns>The bonus reward ID for the stage</returns>
        private static int GetBonusRewardId(int stageId)
        {
            if (!GameData.Instance.EventDungeonStageTable.TryGetValue(stageId, out EventDungeonStageRecord? eventStage))
            {
                log.Error($"EventDungeonStageTable not found for StageId: {stageId}");
                return 0;
            }
            EventDungeonDifficultRecord? difficult = GameData.Instance.EventDungeonDifficultTable.Values.FirstOrDefault(x => x.StageGroup == eventStage.Group);
            if (difficult == null)
            {
                log.Error($"EventDungeonDifficultTable not found for Group: {eventStage.Group}");
                return 0;
            }
            EventDungeonRecord? dungeon = GameData.Instance.EventDungeonTable.Values.FirstOrDefault(x => x.DifficultGroup == difficult.Group);
            if (dungeon == null)
            {
                log.Error($"EventDungeonTable not found for DifficultGroup: {difficult.Group}");
                return 0;
            }
            return dungeon.BonusRewardId;
        }
        
    }
}