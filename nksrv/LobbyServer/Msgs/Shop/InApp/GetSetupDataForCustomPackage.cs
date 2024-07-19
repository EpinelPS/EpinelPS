using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/custompackage/getsetupdata")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCustomPackageSetupData>();

            var response = new ResGetCustomPackageSetupData();

            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
