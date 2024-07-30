using Google.Protobuf;
using EpinelPS.Net;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
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
            r.FileSize = GameData.Instance.Size;
            r.Sha256Sum = ByteString.CopyFrom(GameData.Instance.Sha256Hash);
            r.Salt1 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt1));
            r.Salt2 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt2));

            await WriteDataAsync(r);
        }
    }
}
