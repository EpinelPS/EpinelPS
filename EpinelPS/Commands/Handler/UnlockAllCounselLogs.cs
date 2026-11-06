using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class UnlockAllCounselLogsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class UnlockAllCounselLogsHandler(IExecutionContext context) : BaseHandler<UnlockAllCounselLogsParameter>(context)
{
    public override string Name => "unlock-all-counsel-logs";
    public override string Description => "Unlock all counsel logs for the selected user's owned characters without granting rewards";

    protected override Task<HandleResult> ExecuteAsync(UnlockAllCounselLogsParameter parameters)
    {
        if (context.SelectedUser == null)
            return Task.FromResult(new HandleResult(false, "No user selected"));

        var response = AdminCommands.UnlockAllCounselLogs(context.SelectedUser);
        return Task.FromResult(response.ok
            ? new HandleResult(true, "Counsel logs unlocked successfully")
            : new HandleResult(false, response.error));
    }
}
