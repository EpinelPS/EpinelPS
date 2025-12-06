using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Hexacode;

[PacketPath("/hexacode/get-all")]
public class GetAll : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqGetHexaAll req = await ReadData<ReqGetHexaAll>();
        User user = GetUser();

        ResGetHexaAll response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
