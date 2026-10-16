using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddDevCharactersParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class AddDevCharactersHandler(IExecutionContext context) : BaseHandler<AddDevCharactersParameter>(context)
{
    public override string Name => "add-dev-characters";
    public override string Description => "Add April Fools novelty characters (Shifty, Syuen, etc.), Rare Dorothy, and dev units to the selected user";

    protected async override Task<HandleResult> ExecuteAsync(AddDevCharactersParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddDevCharacters(context.SelectedUser);
        return rsp.ok
            ? new HandleResult(true, "Dev and April Fools characters added successfully")
            : new HandleResult(false, rsp.error);
    }
}
