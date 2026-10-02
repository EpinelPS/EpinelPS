namespace EpinelPS.LobbyServer;

/// <summary>
/// Represents that this class handles a message
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GameRequestAttribute(string url) : Attribute
{
    public string Url { get; set; } = url;
}
