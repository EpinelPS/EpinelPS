using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class AddAllCollectionsParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class AddAllCollectionsHandler(IExecutionContext context) : BaseHandler<AddAllCollectionsParameter>(context)
{
    public override string Name => "add-all-collections";
    public override string Description => "Add all missing collections to the selected user";

    protected async override Task<HandleResult> ExecuteAsync(AddAllCollectionsParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddAllCollections(context.SelectedUser);
        return rsp.ok
            ? new HandleResult(true, "All collections added succesfully")
            : new HandleResult(false, rsp.error);
    }

}