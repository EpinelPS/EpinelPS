using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/set/cutscene/confirmed")]
    public class SetAzxCutSceneConfirmed : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqEnterMiniGameAzx Fields
            //  int AzxId
            //  RepeatedField<int> ConfirmedCutSceneIdList
            ReqSetMiniGameAzxCutSceneConfirmed req = await ReadData<ReqSetMiniGameAzxCutSceneConfirmed>();
            User user = GetUser();

            ResSetMiniGameAzxCutSceneConfirmed response = new();
            
            if (req.AzxId > 0 && req.ConfirmedCutSceneIdList.Count > 0)
                AzxHelper.SetCutSceneConfirmed(user, req.AzxId, [.. req.ConfirmedCutSceneIdList]);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}