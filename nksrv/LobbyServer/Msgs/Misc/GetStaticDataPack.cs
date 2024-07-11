using Google.Protobuf;
using nksrv.StaticInfo;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/staticdatapack")]
    public class GetStaticDataPack : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<StaticDataPackRequest>();

            var r = new StaticDataPackResponse();
            r.Url = StaticDataParser.StaticDataUrl;
            r.Version = StaticDataParser.Version;
            r.Size = StaticDataParser.Size;

            // TODO: Read the file and compute these values
            r.Sha256Sum = ByteString.CopyFrom(StaticDataParser.Sha256Sum);
            r.Salt1 = ByteString.CopyFrom(StaticDataParser.Salt1);
            r.Salt2 = ByteString.CopyFrom(StaticDataParser.Salt2);

          await  WriteDataAsync(r);
        }
    }
}
