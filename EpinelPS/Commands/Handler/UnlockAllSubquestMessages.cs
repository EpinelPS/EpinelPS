using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class UnlockAllSubquestMessagesParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class UnlockAllSubquestMessagesHandler(IExecutionContext context) : BaseHandler<UnlockAllSubquestMessagesParameter>(context)
{
    public override string Name => "unlock-all-subquest-messages";
    public override string Description => "CHEAT: enroll all subquests and create their starting Messenger messages";

    protected async override Task<HandleResult> ExecuteAsync(UnlockAllSubquestMessagesParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        RunCmdResponse response = AdminCommands.UnlockAllSubquestMessages(context.SelectedUser.ID);
        return response.ok
            ? new HandleResult(true, "All subquest Messenger messages unlocked")
            : new HandleResult(false, response.error);
    }
}
