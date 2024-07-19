using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/get")]
    public class GetStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetStageData>();
            var user = GetUser();

            var response = new ResGetStageData();

            response.Field = CreateFieldInfo(user, req.Chapter - 1, req.Mod == 0 ? "Normal" : "Hard");

            response.HasChapterBossEntered = true;

            response.SquadData = "";

            await WriteDataAsync(response);
        }

        public static NetFieldObjectData CreateFieldInfo(Utils.User user, int chapter, string mod)
        {
            var f = new NetFieldObjectData();
            bool found = false;
            string key = chapter + "_" + mod;
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
                    break;
                }
            }

            if (!found)
            {
                user.FieldInfoNew.Add(key, new FieldInfoNew());
                return CreateFieldInfo(user, chapter, mod);
            }

            return f;
        }
    }
}
