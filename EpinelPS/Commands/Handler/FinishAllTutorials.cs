using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class FinishAllTutorialsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class FinishAllTutorialsHandler(IExecutionContext context) : BaseHandler<FinishAllTutorialsParameter>(context)
{
    public override string Name => "finish-all-tutorials";
    public override string Description => "Finish all tutorials for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(FinishAllTutorialsParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.FinishAllTutorials(context.SelectedUser);
        return rsp.ok
            ? new HandleResult(true, "All tutorials finished successfully")
            : new HandleResult(false, rsp.error);
    }
}
