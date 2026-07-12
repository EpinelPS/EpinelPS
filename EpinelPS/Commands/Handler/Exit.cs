using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class ExitParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class ExitHandler(IExecutionContext context) : BaseHandler<ExitParameter>(context)
{
    public override string Name => "exit";
    public override string Description => "Exit the application";

    protected async override Task<HandleResult> ExecuteAsync(ExitParameter parameters)
    {
        Environment.Exit(0);
        return new HandleResult(true);
    }
}
