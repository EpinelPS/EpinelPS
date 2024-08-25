using Google.Protobuf;
using EpinelPS.Database;
using EpinelPS.Utils;

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
            foreach (var item in JsonDb.Instance.GameClientTokens)
            {
                if (item.Key == authToken)
                {
                    UserId = item.Value.UserId;
                }
            }
            if (UserId == 0) throw new Exception("403");
            await HandleAsync();
        }

        protected abstract Task HandleAsync();

        protected async Task WriteDataAsync<T>(T data) where T : IMessage, new()
        {
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
                bool encrypted = false;
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
                var bin = await PacketDecryption.DecryptOrReturnContentAsync(ctx);

                // return grpc IMessage from byte array with type T
                T msg = new();
                msg.MergeFrom(bin.Contents);

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
