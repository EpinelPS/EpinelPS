namespace EpinelPS.Commands.Core;

public interface IExecutionContext
{
    GameUser? SelectedUser { get; set; }
    void Save();
}
