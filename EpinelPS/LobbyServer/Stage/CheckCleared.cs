using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/checkclear")]
    public class CheckCleared : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckStageClear req = await ReadData<ReqCheckStageClear>();

            ResCheckStageClear response = new();
            User user = GetUser();

            foreach (KeyValuePair<string, FieldInfoNew> fields in user.FieldInfoNew)
            {
                foreach (int stages in fields.Value.CompletedStages)
                {
                    if (req.StageIds.Contains(stages))
                        response.ClearedStageIds.Add(stages);
                }
            }

            await WriteDataAsync(response);
        }
    }

}
