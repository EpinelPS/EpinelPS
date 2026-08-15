namespace EpinelPS.Commands.Core;

public interface IHandler : IHandlerInfo
{
    Task<HandleResult> ExecuteAsync(string[] args);
}
