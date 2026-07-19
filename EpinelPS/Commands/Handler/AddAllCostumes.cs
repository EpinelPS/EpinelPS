using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddAllCostumesParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class AddAllCostumesHandler(IExecutionContext context) : BaseHandler<AddAllCostumesParameter>(context)
{
    public override string Name => "add-all-costumes";
    public override string Description => "Add all missing costumes to the selected user";

    protected async override Task<HandleResult> ExecuteAsync(AddAllCostumesParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddAllCostumes(context.SelectedUser);
        return rsp.ok
            ? new HandleResult(true, "All costumes added successfully")
            : new HandleResult(false, rsp.error);
    }
}
