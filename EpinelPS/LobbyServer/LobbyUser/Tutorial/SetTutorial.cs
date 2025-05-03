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
            var req = await ReadData<ReqSetTutorial>();
            var user = GetUser();

            if (!user.ClearedTutorialData.ContainsKey(req.LastClearedTid))
            {
                var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
                cleared.id = req.LastClearedTid;
                user.ClearedTutorialData.Add(req.LastClearedTid, cleared);
            }
            JsonDb.Save();

            var response = new ResSetTutorial();
            await WriteDataAsync(response);
        }
    }
}
