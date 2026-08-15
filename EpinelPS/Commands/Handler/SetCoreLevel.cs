using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SetCoreLevelParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "level", "The core level to set (from 0 to 11)")
            .WithConstraint(new IntRangeConstraint(0, 11)),
    ];

    public int Level { get; init; }
}

public class SetCoreLevelHandler(IExecutionContext context) : BaseHandler<SetCoreLevelParameter>(context)
{
    public override string Name => "set-core-level";
    public override string Description => "Set the core level for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(SetCoreLevelParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.SetCoreLevel(context.SelectedUser, parameters.Level);
        JsonDb.Save();
        return rsp.ok
            ? new HandleResult(true, $"Core level set to {parameters.Level} successfully")
            : new HandleResult(false, rsp.error);
    }
}
