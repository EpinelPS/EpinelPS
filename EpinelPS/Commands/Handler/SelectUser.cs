using EpinelPS.Database;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SelectUserParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.ULong(0, "userId", "The ID of the user to select"),
    ];

    public ulong UserId { get; init; }
}

public class SelectUserHandler(IExecutionContext context) : BaseHandler<SelectUserParameter>(context)
{
    public override string Name => "select-user";
    public override string Description => "Select a user by ID";
    public override string[] Alias => ["user"];

    protected async override Task<HandleResult> ExecuteAsync(SelectUserParameter parameters)
    {
        var user = JsonDb.Instance.Users.FirstOrDefault(u => u.ID == parameters.UserId);
        if (user == null)
            return new HandleResult(false, $"User with ID {parameters.UserId} does not exist");

        context.SelectedUser = user;
        return new HandleResult(true, $"User {user.Username} selected");
    }
}
