using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/staticdatapack")]
    public class GetStaticDataPack : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqStaticDataPackInfo>();

            var r = new ResStaticDataPackInfo();
            r.Url = GameConfig.Root.StaticData.Url;
            r.Version = GameConfig.Root.StaticData.Version;
            r.Size = GameData.Instance.Size;
            r.Sha256Sum = ByteString.CopyFrom(GameData.Instance.Sha256Hash);
            r.Salt1 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt1));
            r.Salt2 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt2));

            await WriteDataAsync(r);
        }
    }
}
