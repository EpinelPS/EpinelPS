using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/recycleroom/get")]
    public class GetRecycleRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetRecycleRoomData>();
            var user = GetUser();
            var response = new ResGetRecycleRoomData();

            response.Recycle.AddRange(user.ResearchProgress.Select(progress => ToNetRecycleRoomData(progress.Value)));

            await WriteDataAsync(response);
        }

        private NetUserRecycleRoomData ToNetRecycleRoomData(RecycleRoomResearchProgress progress)
        {
            return new NetUserRecycleRoomData()
            {
                Tid = progress.Tid,
                Lv = progress.Level,
                Exp = progress.Exp
            };
        }
    }
}
