using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/set/character/unlocked")]
    public class SetAzxCharacterUnlocked : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqSetMiniGameAzxCharacterUnlocked Fields
            //  int AzxId
            //  int CharacterId
            ReqSetMiniGameAzxCharacterUnlocked req = await ReadData<ReqSetMiniGameAzxCharacterUnlocked>();
            User user = GetUser();

            ResSetMiniGameAzxCharacterUnlocked response = new();
            
            if (req.CharacterId > 0 && req.AzxId > 0)
                AzxHelper.SetCharacterUnlocked(user, req.AzxId, req.CharacterId);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}