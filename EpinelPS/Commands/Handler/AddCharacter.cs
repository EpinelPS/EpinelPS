using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddCharacterParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "characterId", "The ID of the character to add"),
    ];

    public int CharacterId { get; init; }
}

public class AddCharacterHandler(IExecutionContext context) : BaseHandler<AddCharacterParameter>(context)
{
    public override string Name => "add-character";
    public override string Description => "Add a character to the selected user";

    protected async override Task<HandleResult> ExecuteAsync(AddCharacterParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var rsp = AdminCommands.AddCharacter(context.SelectedUser, parameters.CharacterId);
        JsonDb.Save();
        return rsp.ok
            ? new HandleResult(true, $"Added character {parameters.CharacterId} successfully")
            : new HandleResult(false, rsp.error);
    }
}
