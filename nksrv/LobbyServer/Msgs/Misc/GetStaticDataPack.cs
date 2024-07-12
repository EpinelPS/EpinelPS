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
            r.Url = GameConfig.Root.StaticData.Url;
            r.Version = GameConfig.Root.StaticData.Version;
            r.Size = StaticDataParser.Instance.Size;
            r.Sha256Sum = ByteString.CopyFrom(StaticDataParser.Instance.Sha256Hash);
            r.Salt1 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt1));
            r.Salt2 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt2));

            await WriteDataAsync(r);
        }
    }
}
