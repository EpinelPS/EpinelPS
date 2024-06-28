using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using nksrv.Utils;

namespace nksrv.IntlServer
{
    /// <summary>
    /// redirector for /v2/ endpoint
    /// </summary>
    internal class IntlAwsNaRedirect : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            Console.WriteLine("AWS NA redirect in: " + Content);
            HttpClientHandler handler = new()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
                AllowAutoRedirect = true // from gameassembly dll
            };

            HttpClient client = new(new LoggingHttpHandler(handler));
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("*/*"));//ACCEPT header
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
            client.DefaultRequestHeaders.Connection.Add("Keep-Alive");


            //  client.DefaultRequestHeaders.Remove("User-agent");

            HttpRequestMessage request = new(HttpMethod.Post, "https://50.18.221.30" + ctx.Request.RawUrl);
            request.Version = HttpVersion.Version11;
            request.Headers.TryAddWithoutValidation("Host", "aws-na.intlgame.com");

            var systemContent = new StringContent(Content);
            systemContent.Headers.Remove("Content-Type");
            systemContent.Headers.Add("Content-Type", "application/json;charset=UTF-8");
            systemContent.Headers.Add("Content-Length", ctx.Request.ContentLength64.ToString());

            request.Content = systemContent;// CONTENT-TYPE header


            var result = await client.SendAsync(request);
            var s = await result.Content.ReadAsStringAsync();
            Console.WriteLine("Redirect out: " + s);
            WriteJsonString(s);
        }
    }
}
