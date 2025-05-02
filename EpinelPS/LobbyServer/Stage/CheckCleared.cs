using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/checkclear")]
    public class CheckCleared : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckStageClear>();

            var response = new ResCheckStageClear();
            var user = GetUser();

            foreach (var fields in user.FieldInfoNew)
            {
                foreach (var stages in fields.Value.CompletedStages)
                {
                    if (req.StageIds.Contains(stages))
                        response.ClearedStageIds.Add(stages);
                }
            }

            await WriteDataAsync(response);
        }
    }

}
