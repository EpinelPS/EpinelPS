using EpinelPS.Database;

namespace EpinelPS.Commands.Core;

public class CliContext : IExecutionContext
{
    public User? SelectedUser { get; set; }
    public void Save() => JsonDb.Save();
}
