using EmbedIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    public abstract class IntlMsgHandler
    {
        protected IHttpContext ctx;
        protected string Content = "";
        protected User? User;
        protected string? Seq;
        protected AccessToken? UsedToken;
        public abstract bool RequiresAuth { get; }
        public async Task HandleAsync(IHttpContext ctx)
        {
            this.ctx = ctx;
            Content = await ctx.GetRequestBodyAsStringAsync();
            Seq = ctx.GetRequestQueryData().Get("seq");
            if (RequiresAuth)
            {
                var x = JsonConvert.DeserializeObject<AuthPkt>(Content);
                string tokToCheck = "";
                if(x != null && x.channel_info != null && !string.IsNullOrEmpty(x.channel_info.account_token))
                {
                    tokToCheck = x.channel_info.account_token;
                }
                else
                {
                    var x2 = JsonConvert.DeserializeObject<AuthPkt2>(Content);
                    if (x2 != null)
                    {
                        tokToCheck = x2.token;
                    }
                }

                if (string.IsNullOrEmpty(tokToCheck))
                    throw new Exception("missing auth token");

                if (x != null)
                {
                    foreach (var tok in JsonDb.Instance.LauncherAccessTokens)
                    {
                        if (tok.Token == tokToCheck)
                        {
                            var user = JsonDb.Instance.Users.Find(x => x.ID == tok.UserID);
                            if (user != null)
                            {
                                UsedToken = tok;
                                User = user;
                            }
                        }
                    }

                    if (User == null)
                    {
                        await WriteJsonStringAsync("{\"msg\":\"expired verify_code!\",\"ret\":2022,\"seq\":\"" + Seq + "\"}\r\n");
                        return;
                    }
                }
                else
                {
                    await WriteJsonStringAsync("{\"msg\":\"expired verify_code!\",\"ret\":2022,\"seq\":\"" + Seq + "\"}\r\n");
                    return;
                }
            }

            await HandleAsync();
        }
        protected abstract Task HandleAsync();

        protected async Task WriteJsonStringAsync(string data)
        {
            if (ctx != null)
            {
                var bt = Encoding.UTF8.GetBytes(data);
                ctx.Response.ContentEncoding = null;
                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentLength64 = bt.Length;
                await ctx.Response.OutputStream.WriteAsync(bt, 0, bt.Length, ctx.CancellationToken);
                await ctx.Response.OutputStream.FlushAsync();
            }
        }
        public class ChannelInfo
        {
            public string openid { get; set; } = "";
            public string token { get; set; } = "";
            public int account_type { get; set; }
            public string account { get; set; } = "";
            public string phone_area_code { get; set; } = "";
            public int account_plat_type { get; set; }
            public string lang_type { get; set; } = "";
            public bool is_login { get; set; }
            public string account_uid { get; set; } = "";
            public string account_token { get; set; } = "";
        }
        public class AuthPkt
        {
            public ChannelInfo channel_info { get; set; } = new();
        }
        public class AuthPkt2
        {
            public string token = "";
            public string openid = "";
            public string account_token = "";
        }
    }
}
