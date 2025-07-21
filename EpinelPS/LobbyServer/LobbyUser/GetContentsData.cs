using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/getcontentsdata")]
    public class GetContentsData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetContentsOpenData req = await ReadData<ReqGetContentsOpenData>();
            Database.User user = GetUser();

            // this request returns a list of "special" stages that mark when something is unlocked, ex: the shop or interception

            List<int> specialStages = [6003003, 6002008, 6002016, 6005003, 6003021, 6011018, 6007021, 6004018, 6005013, 6003009, 6003012, 6009017, 6016039, 6001004, 6000003, 6000001, 6002001, 6004023, 6005026, 6020050, 6006004, 6006023,6022049];

            ResGetContentsOpenData response = new();
            foreach (Database.FieldInfoNew field in user.FieldInfoNew.Values)
            {
                foreach (int stage in field.CompletedStages)
                {
                    if (specialStages.Contains(stage))
                        response.ClearStageList.Add(stage);
                }
            }
            response.MaxGachaCount = 10;
			response.MaxGachaPremiumCount = 10;
            // todo tutorial playcount of gacha
            response.TutorialGachaPlayCount = user.GachaTutorialPlayCount;

            await WriteDataAsync(response);
        }
    }
}
