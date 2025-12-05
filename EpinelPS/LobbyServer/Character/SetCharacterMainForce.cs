using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/mainforce/set")]
    public class SetCharacterMainForce : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetCharacterMainForce req = await ReadData<ReqSetCharacterMainForce>();
            User user = GetUser();

            foreach (CharacterModel item in user.Characters)
            {
                if (item.Csn == req.Csn)
                {
                    item.IsMainForce = req.IsMainForce;
                    break;
                }
            }
            JsonDb.Save();

            ResSetCharacterMainForce response = new();

            await WriteDataAsync(response);
        }
    }
}
