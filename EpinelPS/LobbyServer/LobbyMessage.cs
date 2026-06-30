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
    public byte[] ReturnBytes = [];
    public byte[] Contents = [];

    public User User
    {
        get
        {
            if (UserId == 0) throw new UnauthorizedAccessException();

            return JsonDb.GetUser(UserId) ?? throw new InvalidDataException();
        }
    }

    /// <summary>
    /// Call before calling HandleAsync
    /// </summary>
    public void Reset()
    {
        Contents = [];
        ReturnBytes = [];
        UserId = 0;
        ctx = null;
    }

    public async Task HandleAsync(HttpContext ctx)
    {
        this.ctx = ctx;

        await HandleAsync();
    }
    public async Task HandleAsync(string authToken)
    {
      
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
        PrintMessage(data);
        Logging.WriteLine("", LogType.Debug);

        if (ctx == null)
        {
            using MemoryStream ms = new();
            using CodedOutputStream x2 = new(ms);
            data.WriteTo(x2);
            x2.Flush();
            ReturnBytes = ms.ToArray();
            return;
        }
        else
        {
            ctx.Response.ContentType = "application/octet-stream+protobuf";
            ctx.Response.ContentLength = data.CalculateSize();
            using CodedOutputStream x = new(ctx.Response.Body);
            data.WriteTo(x);

            x.Flush();
        }
    }
    protected async Task<T> ReadData<T>() where T : IMessage, new()
    {
        if (ctx == null)
        {
            T msg2 = new();
            msg2.MergeFrom(Contents);

            Logging.WriteLine("Reading " + msg2.GetType().Name, LogType.Debug);
            if (msg2.GetType().Name != "ReqSyncBadge")
            {
                PrintMessage(msg2);
                Logging.WriteLine("", LogType.Debug);
            }

            return msg2;
        }
        else
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
