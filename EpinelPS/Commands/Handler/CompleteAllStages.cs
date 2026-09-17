using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class CompleteAllStagesParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

/// <summary>
/// Completes campaign stages through the existing AdminCommands implementation.
/// The legacy no-hyphen name remains an alias for old cached admin pages.
/// </summary>
public class CompleteAllStagesHandler(IExecutionContext context) : BaseHandler<CompleteAllStagesParameter>(context)
{
    public override string Name => "complete-all-stages";
    public override string[] Alias => ["completeallstages"];
    public override string Description => "Complete all campaign stages for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(CompleteAllStagesParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        RunCmdResponse response = AdminCommands.CompleteAllStages(context.SelectedUser.ID);
        return response.ok
            ? new HandleResult(true, "All campaign stages completed successfully")
            : new HandleResult(false, response.error);
    }
}
