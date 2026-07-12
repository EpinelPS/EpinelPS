namespace EpinelPS.Commands.Core;

public interface IExecutionContext
{
    User? SelectedUser { get; set; }
    void Save();
}
