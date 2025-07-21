using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/list")]
    public class ListSideStory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListSideStory req = await ReadData<ReqListSideStory>();
            Database.User user = GetUser();

            ResListSideStory response = new();

            foreach (int item in user.CompletedSideStoryStages)
            {
                // TODO cleared at
                response.SideStoryStageDataList.Add(new NetSideStoryStageData() { SideStoryStageId = item, ClearedAt = Timestamp.FromDateTime(DateTime.UtcNow) });
            }

            await WriteDataAsync(response);
        }
    }
}
