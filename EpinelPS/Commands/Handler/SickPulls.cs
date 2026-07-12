using EpinelPS.Database;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SickPullsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class SickPullsHandler(IExecutionContext context) : BaseHandler<SickPullsParameter>(context)
{
    public override string Name => "sick-pulls";
    public override string Description => "Toggle equal chances for all characters to be pulled";

    protected async override Task<HandleResult> ExecuteAsync(SickPullsParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var curSickPulls = JsonDb.IsSickPulls(context.SelectedUser);
        context.SelectedUser.sickpulls = !curSickPulls;
        JsonDb.Save();
        return new HandleResult(true, $"Sick pulls set to {!curSickPulls} for user {context.SelectedUser.Username}");
    }
}
