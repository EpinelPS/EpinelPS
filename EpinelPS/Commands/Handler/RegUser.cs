using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class RegUserParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.String(0, "email", "The email account of the new user"),
        Param.String(1, "password", "The password for the new user"),
    ];

    public string Email { get; init; }
    public string Password { get; init; }
}

public class RegUserHandler(IExecutionContext context) : BaseHandler<RegUserParameter>(context)
{
    public override string Name => "reg-user";
    public override string Description => "Register a new user in the database";

    protected async override Task<HandleResult> ExecuteAsync(RegUserParameter parameters)
    {
        if (JsonDb.Instance.Users.Any(u => u.Username.Equals(parameters.Email, StringComparison.OrdinalIgnoreCase)))
            return new HandleResult(false, $"User with email '{parameters.Email}' already exists");

        var newUser = new User
        {
            ID = GenerateUniqueUserId(),
            Username = parameters.Email,
            Password = parameters.Password,
            RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            PlayerName = "Player_" + Rng.RandomString(8),
            IsAdmin = JsonDb.Instance.Users.Count == 0
        };

        JsonDb.Instance.Users.Add(newUser);
        JsonDb.Save();

        return new HandleResult(true, $"User '{parameters.Email}' registered successfully with ID {newUser.ID}");
    }

    private static ulong GenerateUniqueUserId()
    {
        ulong userId;
        do
        {
            userId = (ulong)new Random().Next(1, int.MaxValue);
        } while (JsonDb.Instance.Users.Any(u => u.ID == userId));
        return userId;
    }
}
