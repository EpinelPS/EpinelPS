using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/buildingisdone")]
    public class SetBuildingDone : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBuildingIsDone req = await ReadData<ReqBuildingIsDone>();
            Database.User user = GetUser();

            ResBuildingIsDone response = new();
          


            await WriteDataAsync(response);
        }
    }
}
