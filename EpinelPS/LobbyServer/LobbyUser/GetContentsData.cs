using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/getcontentsdata")]
    public class GetContentsData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetContentsOpenData>();
            User user = GetUser();

            // this request returns a list of "special" stages that mark when something is unlocked, ex: the shop or interception

            ResGetContentsOpenData response = new();

            foreach (var item in GameData.Instance.ContentsOpenTable)
            {
                foreach (var condition in item.Value.OpenCondition)
                {
                    if (condition.OpenConditionType == ContentsOpenCondition.StageClear)
                    {
                        if (user.IsStageCompleted(condition.OpenConditionValue))
                        {
                            response.ClearStageList.Add(condition.OpenConditionValue);
                        }
                    }
                }

            }
            response.MaxGachaCount = user.GachaTutorialPlayCount;
            response.MaxGachaPremiumCount = user.GachaTutorialPlayCount;
            // todo tutorial playcount of gacha
            response.TutorialGachaPlayCount = user.GachaTutorialPlayCount;

            // ClearSimRoomChapterList: 已通关的章节列表，用于显示超频选项 SimRoomOC
            response.ClearSimRoomChapterList.AddRange(GetClearSimRoomChapterList(user));

            await WriteDataAsync(response);
        }

        private static List<int> GetClearSimRoomChapterList(User user)
        {
            var clearSimRoomChapterList = new List<int>();
            try
            {
                var currentDifficulty = user.ResetableData.SimRoomData?.CurrentDifficulty ?? 0;
                var currentChapter = user.ResetableData.SimRoomData?.CurrentChapter ?? 0;
                if (currentDifficulty > 0 && currentChapter > 0)
                {
                    var chapters = GameData.Instance.SimulationRoomChapterTable.Values.Where(c => c.DifficultyId <= currentDifficulty).ToList();
                    foreach (var chapter in chapters)
                    {
                        bool isAdd = chapter.DifficultyId < currentDifficulty ||
                            (chapter.DifficultyId == currentDifficulty && chapter.Chapter <= currentChapter);
                        if (isAdd) clearSimRoomChapterList.Add(chapter.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn($"GetClearSimRoomChapterList error: {e.Message}");
            }
            return clearSimRoomChapterList;
        }
    }
}
