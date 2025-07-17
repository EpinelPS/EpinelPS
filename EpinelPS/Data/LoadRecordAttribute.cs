namespace EpinelPS.Data
{
    [System.AttributeUsage(System.AttributeTargets.Field)]

    public class LoadRecordAttribute(string file, string primaryKey) : Attribute
    {
        public string File { get; set; } = file;
        public string PrimaryKey { get; set; } = primaryKey;
    }
}