using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SetLevelParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "level", "The level to set (from 1 to 999)")
            .WithConstraint(new IntRangeConstraint(1, 999)),
    ];

    public int Level { get; init; }
}

public class SetLevelHandler(IExecutionContext context) : BaseHandler<SetLevelParameter>(context)
{
    public override string Name => "set-level";
    public override string Description => "Set the level for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(SetLevelParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.SetCharacterLevel(context.SelectedUser, parameters.Level);
        JsonDb.Save();
        return rsp.ok
            ? new HandleResult(true, $"Level set to {parameters.Level} successfully")
            : new HandleResult(false, rsp.error);
    }
}
