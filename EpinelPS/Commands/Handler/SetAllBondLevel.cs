using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class SetAllBondLevelParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "bondLevel", "The bond level of all the charactors (from 1 to 30)")
            .WithConstraint(new IntRangeConstraint(1, 30)),
    ];

    public int BondLevel { get; init; }
}

public class SetAllBondLevelHandler(IExecutionContext context) : BaseHandler<SetAllBondLevelParameter>(context)
{
    public override string Name => "set-all-bond-level";
    public override string Description => "Set all charactors' bond level for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(SetAllBondLevelParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.SetAllBondLevel(context.SelectedUser, parameters.BondLevel);
        return rsp.ok
            ? new HandleResult(true, $"All charactors' bond level set to {parameters.BondLevel} succesfully")
            : new HandleResult(false, rsp.error);
    }

}