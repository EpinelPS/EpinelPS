using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class CompleteStageParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "num1", "The chapter number"),
        Param.Int(1, "num2", "The stage number"),
    ];

    public int Num1 { get; init; }
    public int Num2 { get; init; }
}

public class CompleteStageHandler(IExecutionContext context) : BaseHandler<CompleteStageParameter>(context)
{
    public override string Name => "complete-stage";
    public override string Description => "Complete a stage for the selected user";

    protected async override Task<HandleResult> ExecuteAsync(CompleteStageParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.CompleteStage(context.SelectedUser.ID, $"{parameters.Num1}-{parameters.Num2}");
        return rsp.ok
            ? new HandleResult(true, $"Stage {parameters.Num1}-{parameters.Num2} completed successfully")
            : new HandleResult(false, rsp.error);
    }
}
