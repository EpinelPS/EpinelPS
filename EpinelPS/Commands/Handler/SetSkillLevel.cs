using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SetSkillLevelParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "level", "The skill level to set (from 1 to 10)")
            .WithConstraint(new IntRangeConstraint(1, 10)),
    ];

    public int Level { get; init; }
}

public class SetSkillLevelHandler(IExecutionContext context) : BaseHandler<SetSkillLevelParameter>(context)
{
    public override string Name => "set-skill-level";
    public override string Description => "Set all characters' skill levels for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(SetSkillLevelParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.SetSkillLevel(context.SelectedUser, parameters.Level);
        JsonDb.Save();
        return rsp.ok
            ? new HandleResult(true, $"Set all characters' skills to level {parameters.Level} successfully")
            : new HandleResult(false, rsp.error);
    }
}
