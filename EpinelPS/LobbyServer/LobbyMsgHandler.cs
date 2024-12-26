using Google.Protobuf;
using EpinelPS.Database;
using EpinelPS.Utils;
using Paseto.Builder;
using Paseto;

namespace EpinelPS.LobbyServer
{
    public abstract class LobbyMsgHandler
    {
        protected HttpContext? ctx;
        protected ulong UserId;
        protected string UsedAuthToken = "";
        public byte[] ReturnBytes = [];
        public byte[] Contents = [];

        /// <summary>
        /// Call before calling HandleAsync
        /// </summary>
        public void Reset()
        {
            Contents = [];
            ReturnBytes = [];
            UsedAuthToken = "";
            UserId = 0;
            ctx = null;
        }

        public async Task HandleAsync(HttpContext ctx)
        {
            this.ctx = ctx;
            if (ctx.Request.Headers.Keys.Contains("Authorization"))
            {
                var token = ctx.Request.Headers.Authorization.FirstOrDefault();
                if (token != null)
                {
                    UsedAuthToken = token;
                }
            }

            await HandleAsync();
        }
        public async Task HandleAsync(string authToken)
        {
            this.UsedAuthToken = authToken;

            var encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                             .Decode(authToken, new PasetoTokenValidationParameters() { ValidateLifetime = true});

            UserId = ((System.Text.Json.JsonElement)encryptionToken.Paseto.Payload["userid"]).GetUInt64();

            if (UserId == 0) throw new Exception("403");
            await HandleAsync();
        }

        protected abstract Task HandleAsync();


        private void PrintMessage<T>(T data) where T : IMessage, new()
        {
            var str = (string)data.GetType().InvokeMember("ToString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, data, null);
            Console.WriteLine(str);
        }
        protected async Task WriteDataAsync<T>(T data) where T : IMessage, new()
        {
            Console.WriteLine("Writing " + data.GetType().Name);
            PrintMessage(data);
            Console.WriteLine();

            if (ctx == null)
            {
                var ms = new MemoryStream();
                var x2 = new CodedOutputStream(ms);
                data.WriteTo(x2);
                x2.Flush();
                ReturnBytes = ms.ToArray();
                return;
            }
            else
            {
                ctx.Response.ContentType = "application/octet-stream+protobuf";
                ctx.Response.ContentLength = data.CalculateSize();
                bool encrypted = ctx.Request.Headers.ContentEncoding.Contains("enc");
                encrypted = false; //TODO implement, although client does not complain
                var responseBytes = encrypted ? new MemoryStream() : ctx.Response.Body;
                var x = new CodedOutputStream(responseBytes);
                data.WriteTo(x);

                x.Flush();

                if (encrypted)
                {
                    ctx.Response.Headers.ContentEncoding = new Microsoft.Extensions.Primitives.StringValues("gzip,enc");
                    var enc = PacketDecryption.EncryptData(((MemoryStream)responseBytes).ToArray(), UsedAuthToken);
                    await ctx.Response.Body.WriteAsync(enc);
                }
            }
        }
        protected async Task<T> ReadData<T>() where T : IMessage, new()
        {
            if (ctx == null)
            {
                T msg2 = new();
                msg2.MergeFrom(Contents);
                return msg2;
            }
            else
            {
                // return grpc IMessage from byte array with type T
                T msg = new();
                Console.WriteLine("Reading " + msg.GetType().Name);

                var bin = await PacketDecryption.DecryptOrReturnContentAsync(ctx);
                msg.MergeFrom(bin.Contents);

                PrintMessage(msg);
                Console.WriteLine();

                UserId = bin.UserId;
                UsedAuthToken = bin.UsedAuthToken;

                return msg;
            }
        }

        public User GetUser()
        {
            return JsonDb.GetUser(UserId) ?? throw new Exception("null user");
        }
        public User? GetUser(ulong id)
        {
            return JsonDb.GetUser(id);
        }
    }
}
