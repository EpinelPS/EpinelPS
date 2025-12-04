using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/view/set")]
    public class SetViewed : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetViewSideStory req = await ReadData<ReqSetViewSideStory>();
            User user = GetUser();

            ResSetViewSideStory response = new();

            foreach (var id in req.ViewedSideStoryIds)
            {
                if (!user.ViewedSideStoryStages.Contains(id))
                {
                    user.ViewedSideStoryStages.Add(id);
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
