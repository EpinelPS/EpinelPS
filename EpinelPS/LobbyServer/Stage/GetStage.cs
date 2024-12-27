using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/get")]
    public class GetStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStageData req = await ReadData<ReqGetStageData>();
            var user = GetUser();

            ResGetStageData response = new()
            {
                Field = CreateFieldInfo(user, req.Chapter - 1, req.Mod == 0 ? "Normal" : "Hard", out bool bossEntered),
                HasChapterBossEntered = bossEntered,
                SquadData = ""
            };

            await WriteDataAsync(response);
        }

        public static NetFieldObjectData CreateFieldInfo(Database.User user, int chapter, string mod, out bool BossEntered)
        {
            var f = new NetFieldObjectData();
            bool found = false;
            string key = chapter + "_" + mod;
            BossEntered = false;
            foreach (var item in user.FieldInfoNew)
            {
                if (item.Key == key)
                {
                    found = true;
                    foreach (var stage in item.Value.CompletedStages)
                    {
                        f.Stages.Add(new NetFieldStageData() { StageId = stage });
                    }
                    foreach (var obj in item.Value.CompletedObjects)
                    {
                        f.Objects.Add(obj);
                    }
                    BossEntered = item.Value.BossEntered;
                    break;
                }
            }

            if (!found)
            {
                user.FieldInfoNew.Add(key, new FieldInfoNew());
                return CreateFieldInfo(user, chapter, mod, out BossEntered);
            }

            return f;
        }
    }
}
