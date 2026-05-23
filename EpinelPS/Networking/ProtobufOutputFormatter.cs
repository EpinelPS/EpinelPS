using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace EpinelPS.Networking;
public sealed class ProtobufOutputFormatter : OutputFormatter
{
    public ProtobufOutputFormatter()
    {
        SupportedMediaTypes.Add(
            MediaTypeHeaderValue.Parse(
                "application/octet-stream+protobuf"));
    }

    protected override bool CanWriteType(Type? type)
    {
        return typeof(IMessage).IsAssignableFrom(type);
    }

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context)
    {
        var response = context.HttpContext.Response;

        if (context.Object is IMessage message)
        {
            using MemoryStream ms = new(); // Required to determine the amount of bytes written
            using CodedOutputStream x = new(ms);
            message.WriteTo(x);

            x.Flush();
            ms.Flush();

            response.Headers.ContentLength = ms.Length;

            ms.Position = 0;
            ms.CopyTo(response.Body);
        }
        await response.Body.FlushAsync();
    }
}