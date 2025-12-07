using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/set/tutorial/confirmed")]
    public class SetAzxTutorialConfirmed : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqSetMiniGameAzxTutorialConfirmed>();
            User user = GetUser();

            ResSetMiniGameAzxTutorialConfirmed response = new();
            
            AzxHelper.SetTutorialConfirmed(user, 1);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}