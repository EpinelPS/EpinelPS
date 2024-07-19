using nksrv.StaticInfo;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/costume/get")]
    public class GetCharacterCostume : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCharacterCostumeData>();

            var response = new ResGetCharacterCostumeData();

            // return all
            response.CostumeIds.AddRange(StaticDataParser.Instance.GetAllCostumes());

            await WriteDataAsync(response);
        }
    }
}
