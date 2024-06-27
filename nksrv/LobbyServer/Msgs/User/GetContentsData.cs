using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/user/getcontentsdata")]
    public class GetContentsData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetContentsOpenData>();
            var user = GetUser();

            var response = new ResGetContentsOpenData();
            bool includeFirst = true;
            foreach (var field in user.FieldInfo.Values)
            {
                // only include first and last clears
                int i = 0;
                foreach (var stage in field.CompletedStages)
                {
                    if (i == 0 && includeFirst)
                    {
                        response.ClearStageList.Add(stage.StageId);
                        includeFirst = false;
                    }
                    else if (i == field.CompletedStages.Count - 1)
                        response.ClearStageList.Add(stage.StageId);
                    i++;
                }
            }
            response.MaxGachaCount = 10;
            // todo tutorial playcount of gacha

            WriteData(response);
        }
    }
}
