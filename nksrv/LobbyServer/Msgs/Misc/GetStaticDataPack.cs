using Google.Protobuf;
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
            r.Url = "https://cloud.nikke-kr.com/prdenv/121-c5e64b1a1b/staticdata/data/qa-240620-05b-p1/307748/StaticData.pack";
            r.Version = "data/qa-240620-05b-p1/307748";
            r.Size = 11575712;

            // TODO: Read the file and compute these values
            r.Sha256Sum = ByteString.CopyFrom(Convert.FromBase64String("PBcDa3PoHR2MJQ+4Xc3/FUSgkqx2gY25MBJ0ih9FMsM="));
            r.Salt1 = ByteString.CopyFrom(Convert.FromBase64String("WqyrQ8MGtzwHN3AGPkqVKyjdfWZjBJXw9K7nGblv/SA="));
            r.Salt2 = ByteString.CopyFrom(Convert.FromBase64String("6Gf2jEvAX2mt5OWIxIU5uDdbjKtIc+VgTjKKSLuYnsI="));

            WriteData(r);
        }
    }
}
