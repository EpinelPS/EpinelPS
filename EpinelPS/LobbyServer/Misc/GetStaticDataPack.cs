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
            ReqStaticDataPackInfo req = await ReadData<ReqStaticDataPackInfo>();

            ResStaticDataPackInfo r = new()
            {
                Url = GameConfig.Root.StaticData.Url,
                Version = GameConfig.Root.StaticData.Version,
                Size = GameData.Instance.Size,
                Sha256Sum = ByteString.CopyFrom(GameData.Instance.Sha256Hash),
                Salt1 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt1)),
                Salt2 = ByteString.CopyFrom(Convert.FromBase64String(GameConfig.Root.StaticData.Salt2))
            };

            await WriteDataAsync(r);
        }
    }
}
