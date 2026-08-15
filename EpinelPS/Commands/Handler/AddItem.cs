using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddItemParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "itemId", "The ID of the item to add"),
        Param.Int(1, "quantity", "The quantity of the item to add"),
    ];

    public int ItemId { get; init; }
    public int Quantity { get; init; }
}

public class AddItemHandler(IExecutionContext context) : BaseHandler<AddItemParameter>(context)
{
    public override string Name => "add-item";
    public override string Description => "Add an item to the selected user's inventory";

    protected async override Task<HandleResult> ExecuteAsync(AddItemParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false);
        var rsp = AdminCommands.AddItem(context.SelectedUser, parameters.ItemId, parameters.Quantity);
        JsonDb.Save();
        return rsp.ok ? new HandleResult(true) : new HandleResult(false, rsp.error);
    }
}
