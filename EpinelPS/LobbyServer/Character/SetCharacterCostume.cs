using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/costume/set")]
    public class SetCharacterCostume : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetCharacterCostume req = await ReadData<ReqSetCharacterCostume>();
            User user = GetUser();

            foreach (Database.Character item in user.Characters)
            {
                if (item.Csn == req.Csn)
                {
                    item.CostumeId = req.CostumeId;
                    break;
                }
            }
            JsonDb.Save();

            ResSetCharacterCostume response = new();

            await WriteDataAsync(response);
        }
    }
}
