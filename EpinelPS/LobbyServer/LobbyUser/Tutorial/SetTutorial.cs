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
                ClearedTutorialData cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
                cleared.id = req.LastClearedTid;
                user.ClearedTutorialData.Add(req.LastClearedTid, cleared);
            }
            JsonDb.Save();

            ResSetTutorial response = new();
            await WriteDataAsync(response);
        }
    }
}
