using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddAllEquipmentsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "amount1", "Amount of FavoriteItems to add", true),
        Param.Int(1, "amount2", "Amount of consumables to add", true),
        Param.Int(2, "amount3", "Amount of T9 equipment to add", true),
    ];

    public int Amount1 { get; init; }
    public int Amount2 { get; init; }
    public int Amount3 { get; init; }
}

public class AddAllEquipmentsHandler(IExecutionContext context) : BaseHandler<AddAllEquipmentsParameter>(context)
{
    public override string Name => "add-all-equipments";
    public override string Description => "Adds all FavoriteItem, consumables and T9 equipment";

    protected async override Task<HandleResult> ExecuteAsync(AddAllEquipmentsParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddAllEq(context.SelectedUser, parameters.Amount1, parameters.Amount2, parameters.Amount3);
        return rsp.ok
            ? new HandleResult(true, "All equipment added successfully")
            : new HandleResult(false, rsp.error);
    }
}
