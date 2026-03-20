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

            var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
            var tutorial = new ClearedTutorialData()
            {
                ClearedStageId = cleared.ClearedStageId,
                GroupId = cleared.GroupId,
                Id = cleared.Id,
                NextId = cleared.NextId,
                SaveTutorial = cleared.SaveTutorial,
                VersionGroup = cleared.VersionGroup
            };

            if (!user.ClearedTutorialData.ContainsKey(req.LastClearedTid))
            {
                user.ClearedTutorialData.Add(req.LastClearedTid, tutorial);
            }
            else
            {
                user.ClearedTutorialData[req.LastClearedTid] = tutorial;
            }
            JsonDb.Save();

            ResSetTutorial response = new();
            await WriteDataAsync(response);
        }
    }
}
