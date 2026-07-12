using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddAllCharactersParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class AddAllCharactersHandler(IExecutionContext context) : BaseHandler<AddAllCharactersParameter>(context)
{
    public override string Name => "add-all-characters";
    public override string Description => "Add all missing characters to the selected user with default levels and skills";

    protected async override Task<HandleResult> ExecuteAsync(AddAllCharactersParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddAllCharacters(context.SelectedUser);
        return rsp.ok
            ? new HandleResult(true, "All characters added successfully")
            : new HandleResult(false, rsp.error);
    }
}
