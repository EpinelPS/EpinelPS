using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/custompackage/getsetupdata")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCustomPackageSetupData req = await ReadData<ReqGetCustomPackageSetupData>();

            ResGetCustomPackageSetupData response = new();

            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
