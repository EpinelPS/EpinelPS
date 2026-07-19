using System.Text;
using EpinelPS.Database;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class ShowUsersParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class ShowUsersHandler(IExecutionContext context) : BaseHandler<ShowUsersParameter>(context)
{
    public override string Name => "show-users";
    public override string Description => "Display a list of all users in the database";

    protected async override Task<HandleResult> ExecuteAsync(ShowUsersParameter parameters)
    {
        var users = JsonDb.Instance.Users;
        if (users.Count == 0)
            return new HandleResult(true, "No users found in the database.");

        var sb = new StringBuilder();
        sb.AppendLine("ID\t\tUsername\tNickname");
        foreach (var user in users)
        {
            sb.AppendLine($"{user.ID}\t{user.Username}\t{user.Nickname}");
        }
        return new HandleResult(true, sb.ToString().TrimEnd());
    }
}
