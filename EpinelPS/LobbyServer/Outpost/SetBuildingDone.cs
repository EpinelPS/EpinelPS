using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/buildingisdone")]
    public class SetBuildingDone : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBuildingIsDone>();
            var user = GetUser();

            var response = new ResBuildingIsDone();
          


            await WriteDataAsync(response);
        }
    }
}
