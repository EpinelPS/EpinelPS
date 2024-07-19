namespace nksrv.Utils
{
    /// <summary>
    /// Represents that this class handles a message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketPathAttribute : Attribute
    {
        public string Url { get; set; }
        public PacketPathAttribute(string url)
        {
            Url = url;
        }
    }
}
