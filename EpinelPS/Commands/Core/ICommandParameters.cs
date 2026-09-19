using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Core;

public interface ICommandParameters
{
    static abstract ParameterDescriptor[] Descriptors { get; }
}
