namespace EpinelPS.Utils
{
    /// <summary>
    /// Represents that this class handles a message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketPathAttribute(string url) : Attribute
    {
        public string Url { get; set; } = url;
    }
}
