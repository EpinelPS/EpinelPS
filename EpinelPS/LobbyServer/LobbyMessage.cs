using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Paseto;
using Paseto.Builder;

namespace EpinelPS.LobbyServer;

/// <summary>
/// Old way of handling requests. Will be removed in the future and replaced with ASP.NET controllers
/// </summary>
public abstract class LobbyMessage
{
    protected HttpContext? ctx;
    protected ulong UserId;
    public GameContext GameContext = null!;

    public async Task HandleAsync(HttpContext ctx)
    {
        UserId = 0;
        this.ctx = ctx;
        GameContext = ctx.RequestServices.GetRequiredService<GameContext>();
        await HandleAsync();
    }
    protected abstract Task HandleAsync();


    private static void PrintMessage<T>(T data) where T : IMessage, new()
    {
        string? str = (string?)data.GetType().InvokeMember("ToString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, data, null);
        if (str != null)
            Logging.WriteLine(str, LogType.Debug);
    }
    protected async Task WriteDataAsync<T>(T data) where T : IMessage, new()
    {
        Logging.WriteLine("Writing " + data.GetType().Name, LogType.Debug);
        if (data.GetType().Name != "ResGetJupiterProductList")
        {
            PrintMessage(data);
            Logging.WriteLine("", LogType.Debug);
        }

        Logging.WriteLine("", LogType.Debug);

        ctx.Response.ContentType = "application/octet-stream+protobuf";
        ctx.Response.ContentLength = data.CalculateSize();
        using CodedOutputStream x = new(ctx.Response.Body);
        data.WriteTo(x);

        x.Flush();
    }
    protected async Task<T> ReadData<T>() where T : IMessage, new()
    {
        // return grpc IMessage from byte array with type T
        T msg = new();
        Logging.WriteLine("Reading " + msg.GetType().Name, LogType.Debug);

        msg.MergeFrom(ctx.Request.Body);

        if (msg.GetType().Name != "ReqSyncBadge")
        {
            PrintMessage(msg);
            Logging.WriteLine("", LogType.Debug);
        }

        var id = ctx.Items["UserID"];
        if (id != null && id is ulong u)
            UserId = u;

        return msg;
    }

    public User GetUser()
    {
        return JsonDb.GetUser(UserId) ?? throw new UnauthorizedAccessException("Invalid authentication token");
    }
    public User? GetUser(ulong Id)
    {
        return JsonDb.GetUser(Id);
    }

    public RankData GetRank()
    {
        return JsonDb.GetRank();
    }
}
