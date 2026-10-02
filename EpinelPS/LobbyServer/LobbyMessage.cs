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
    private sealed class LobbyCallContext
    {
        public HttpContext? HttpContext;
        public ulong UserId;
        public GameContext? GameContext;
    }

    private static readonly AsyncLocal<LobbyCallContext> _currentContext = new();

    protected HttpContext ctx
    {
        get => _currentContext.Value?.HttpContext ?? throw new InvalidOperationException("No active HttpContext in LobbyMessage");
        set
        {
            var c = _currentContext.Value ??= new();
            c.HttpContext = value;
        }
    }

    protected ulong UserId
    {
        get => _currentContext.Value?.UserId ?? 0;
        set
        {
            var c = _currentContext.Value ??= new();
            c.UserId = value;
        }
    }

    public GameContext GameContext
    {
        get => _currentContext.Value?.GameContext ?? throw new InvalidOperationException("No active GameContext in LobbyMessage");
        set
        {
            var c = _currentContext.Value ??= new();
            c.GameContext = value;
        }
    }

    public async Task HandleAsync(HttpContext ctx)
    {
        var callContext = new LobbyCallContext
        {
            HttpContext = ctx,
            UserId = 0,
            GameContext = ctx.RequestServices.GetRequiredService<GameContext>()
        };
        _currentContext.Value = callContext;
        await HandleAsync();
    }
    protected abstract Task HandleAsync();


    private static void PrintMessage<T>(T data) where T : IMessage, new()
    {
        if (Logging.IsDebugEnabled)
        {
            string? str = data.ToString();
            if (!string.IsNullOrEmpty(str))
                Logging.WriteLine(str, LogType.Debug);
        }
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
