using EpinelPS.Database;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class RemoveUserParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class RemoveUserHandler(IExecutionContext context) : BaseHandler<RemoveUserParameter>(context)
{
    public override string Name => "remove-user";
    public override string Description => "Remove the selected user";

    protected async override Task<HandleResult> ExecuteAsync(RemoveUserParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        Console.WriteLine($"Are you sure you want to remove user {context.SelectedUser.Nickname} (ID: {context.SelectedUser.ID})? This action cannot be undone. (y/n)");
        var confirmation = Console.ReadLine();
        if (confirmation?.ToLower() != "y")
        {
            return new HandleResult(false, "User removal cancelled");
        }

        
        using (var db = GameContext.CreateNew())
        {
            var sdkUser = db.SdkUsers.Find(context.SelectedUser.ID);
            db.SdkUsers.Remove(sdkUser);
            db.Users.Remove(context.SelectedUser);
            
            db.SaveChanges();
        }

        var username = context.SelectedUser.Nickname;
        context.SelectedUser = null;
        return new HandleResult(true, $"User {username} removed successfully");
    }
}
