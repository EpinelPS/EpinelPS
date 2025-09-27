using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/get-static-data-pack-info-mpk")]
    public class GetStaticDataPackNew : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStaticDataPackInfoMpk req = await ReadData<ReqStaticDataPackInfoMpk>();

            StaticData data = GameConfig.Root.StaticDataMpk;

            ResStaticDataPackInfoMpk r = new()
            {
                Url = data.Url,
                Version = data.Version,
                Size = GameData.Instance.MpkSize,
                Sha256Sum = ByteString.CopyFrom(GameData.Instance.MpkHash),
                Salt1 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt1)),
                Salt2 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt2))
            };

            await WriteDataAsync(r);
        }
    }
}
