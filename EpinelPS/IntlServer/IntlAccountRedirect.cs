using EpinelPS.Utils;
using System.Net;
using System.Net.Http.Headers;

namespace EpinelPS.IntlServer
{
    /// <summary>
    /// redirect for /account endponts
    /// </summary>
    public class IntlAccountRedirect : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            if (ctx == null) throw new Exception("ctx cannot be null");
            Console.WriteLine("li-sg redirect in: " + Content);
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

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://49.51.129.135" + ctx.Request.RawUrl);
            request.Version = HttpVersion.Version11;
            request.Headers.TryAddWithoutValidation("Host", "li-sg.intlgame.com");

            var systemContent = new StringContent(Content);
            systemContent.Headers.Remove("Content-Type");
            systemContent.Headers.Add("Content-Type", "application/json");
            systemContent.Headers.Add("Content-Length", ctx.Request.ContentLength64.ToString());

            request.Content = systemContent;// CONTENT-TYPE header


            var result = await client.SendAsync(request);
            var s = await result.Content.ReadAsStringAsync();
            await WriteJsonStringAsync(s);
            Console.WriteLine("li-sg redirect out: " + s);
        }
    }
}
