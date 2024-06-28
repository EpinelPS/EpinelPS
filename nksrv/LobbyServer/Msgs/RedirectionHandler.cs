using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using nksrv.Utils;
using Sodium;
using System.IO.Compression;

namespace nksrv.LobbyServer.Msgs
{
    public class RedirectionHandler : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            byte[] reqBody = ctx == null ? Contents : (await DecryptOrReturnContentAsync(ctx)).Contents;

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true // from gameassembly dll
            };

            HttpClient client = new(new LoggingHttpHandler(handler));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream+protobuf"));
            client.BaseAddress = new Uri("https://global-lobby.nikke-kr.com");
            client.DefaultRequestVersion = HttpVersion.Version20;
            var systemContent = new ByteArrayContent(reqBody);
            systemContent.Headers.Remove("Content-Type");
            if (ctx.Request.ContentLength64 != 0)
                systemContent.Headers.Add("Content-Type", "application/octet-stream+protobuf");
            systemContent.Headers.Add("Content-Length", ctx.Request.ContentLength64.ToString());

            // request.Content = systemContent;// CONTENT-TYPE header


            var result = await client.PostAsync(ctx.Request.RawUrl, systemContent);
            var bt = await result.Content.ReadAsByteArrayAsync();

            ctx.Response.ContentEncoding = null;
            ctx.Response.ContentType = result.Content.Headers.GetValues("Content-Type").First();
            ctx.Response.ContentLength64 = bt.Length;
            ctx.Response.OutputStream.Write(bt, 0, bt.Length);
            ctx.Response.OutputStream.Flush();
        }

        public static async Task<PacketDecryptResponse> DecryptOrReturnContentAsync(IHttpContext ctx)
        {
            using MemoryStream buffer = new MemoryStream();

            var stream = ctx.Request.InputStream;

            var encoding = ctx.Request.Headers[HttpHeaderNames.ContentEncoding]?.Trim();
            Stream decryptedStream = encoding switch
            {
                CompressionMethodNames.Gzip => new GZipStream(stream, CompressionMode.Decompress),
                CompressionMethodNames.Deflate => new DeflateStream(stream, CompressionMode.Decompress),
                CompressionMethodNames.None or null => stream,
                "gzip,enc" => stream,
                _ => throw HttpException.BadRequest($"Unsupported content encoding \"{encoding}\""),
            };
            await stream.CopyToAsync(buffer, 81920, ctx.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return new PacketDecryptResponse() { Contents = buffer.ToArray() };
        }
    }
}
