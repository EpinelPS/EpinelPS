namespace EpinelPS.Commands.Core;

public interface ICommandRegistry
{
    IReadOnlyList<IHandlerInfo> GetHandlers();
    IHandlerInfo? FindHandler(string name);
}
