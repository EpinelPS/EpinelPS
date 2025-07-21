using EpinelPS.Data;
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
            User user = GetUser();

            string mapId = GameData.Instance.GetMapIdFromChapter(req.Chapter, (ChapterMod)req.Mod);

            ResGetStageData response = new()
            {
                Field = CreateFieldInfo(user, mapId, out bool bossEntered),
                HasChapterBossEntered = bossEntered,
                SquadData = ""
            };

            await WriteDataAsync(response);
        }

        public static NetFieldObjectData CreateFieldInfo(User user, string mapId, out bool BossEntered)
        {
            NetFieldObjectData f = new();
            bool found = false;
            BossEntered = false;
            foreach (KeyValuePair<string, FieldInfoNew> item in user.FieldInfoNew)
            {
                if (item.Key == mapId)
                {
                    found = true;
                    foreach (int stage in item.Value.CompletedStages)
                    {
                        f.Stages.Add(new NetFieldStageData() { StageId = stage });
                    }
                    foreach (NetFieldObject obj in item.Value.CompletedObjects)
                    {
                        f.Objects.Add(obj);
                    }
                    BossEntered = item.Value.BossEntered;
                    break;
                }
            }

            if (!found)
            {
                user.FieldInfoNew.Add(mapId, new FieldInfoNew());
                return CreateFieldInfo(user, mapId, out BossEntered);
            }

            return f;
        }
    }
}
