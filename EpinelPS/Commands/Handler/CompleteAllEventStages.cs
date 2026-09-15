using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class CompleteAllEventStagesParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class CompleteAllEventStagesHandler(IExecutionContext context) : BaseHandler<CompleteAllEventStagesParameter>(context)
{
    public override string Name => "complete-all-event-stages";
    public override string Description => "Complete all event stages for the selected user (triggers EventStageClear)";

    protected async override Task<HandleResult> ExecuteAsync(CompleteAllEventStagesParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.CompleteAllEventStages(context.SelectedUser.ID);
        return rsp.ok
            ? new HandleResult(true, "All event stages completed successfully")
            : new HandleResult(false, rsp.error);
    }
}
