using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser.Tutorial
{
    [PacketPath("/tutorial/set")]
    public class SetTutorial : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetTutorial req = await ReadData<ReqSetTutorial>();
            User user = GetUser();

            if (!user.ClearedTutorialData.ContainsKey(req.LastClearedTid))
            {
                var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
                cleared.Id = req.LastClearedTid;
                user.ClearedTutorialData.Add(req.LastClearedTid, new ClearedTutorialData()
                {
                    ClearedStageId = cleared.ClearedStageId,
                    GroupId = cleared.GroupId,
                    Id = cleared.GroupId,
                    NextId = cleared.NextId,
                    SaveTutorial = cleared.SaveTutorial,
                    VersionGroup = cleared.VersionGroup
                });
            }
            JsonDb.Save();

            ResSetTutorial response = new();
            await WriteDataAsync(response);
        }
    }
}
