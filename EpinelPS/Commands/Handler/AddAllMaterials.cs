using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddAllMaterialsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "amount", "Material count (default: 1)", true),
    ];

    public int Amount { get; init; }
}

public class AddAllMaterialsHandler(IExecutionContext context) : BaseHandler<AddAllMaterialsParameter>(context)
{
    public override string Name => "add-all-materials";
    public override string Description => "Add all materials to the selected user";

    protected async override Task<HandleResult> ExecuteAsync(AddAllMaterialsParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var amount = parameters.Amount > 0 ? parameters.Amount : 1;
        var rsp = AdminCommands.AddAllMaterials(context.SelectedUser, amount);
        return rsp.ok
            ? new HandleResult(true, $"All materials added to user '{context.SelectedUser.Username}' successfully")
            : new HandleResult(false, rsp.error);
    }
}
