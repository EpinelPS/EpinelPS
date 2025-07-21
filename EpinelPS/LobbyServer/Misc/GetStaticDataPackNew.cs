using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/get-static-data-pack-info")]
    public class GetStaticDataPackNew : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStaticDataPackInfoV2 req = await ReadData<ReqStaticDataPackInfoV2>();

            Console.WriteLine("Requesting " + req.Type);

            StaticData data = req.Type == StaticDataPackType.Mpk ? GameConfig.Root.StaticDataMpk : GameConfig.Root.StaticData;

            ResStaticDataPackInfoV2 r = new()
            {
                Url = data.Url,
                Version = data.Version,
                Size = req.Type == StaticDataPackType.Mpk ? GameData.Instance.MpkSize : GameData.Instance.Size,
                Sha256Sum = ByteString.CopyFrom(req.Type == StaticDataPackType.Mpk ? GameData.Instance.MpkHash : GameData.Instance.Sha256Hash),
                Salt1 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt1)),
                Salt2 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt2))
            };

            await WriteDataAsync(r);
        }
    }
}
