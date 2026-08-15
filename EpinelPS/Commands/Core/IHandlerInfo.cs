namespace EpinelPS.Commands.Core;

public interface IHandlerInfo
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    string[] Alias { get; }
}
