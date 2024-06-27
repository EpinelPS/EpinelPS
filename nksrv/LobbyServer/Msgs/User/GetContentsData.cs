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
            foreach (var field in user.FieldInfo.Values)
            {
                foreach (var stage in field.CompletedStages)
                {
                    response.ClearStageList.Add(stage.StageId);
                }
            }

            WriteData(response);
        }
    }
}
