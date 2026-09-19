using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;
using System.Security.Cryptography;
using System.Text;

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
    private static readonly MD5 md5 = MD5.Create();

    protected async override Task<HandleResult> ExecuteAsync(RegUserParameter parameters)
    {
        using (var db = GameContext.CreateNew())
        {
            if (db.SdkUsers.Any(u => u.Email == parameters.Email))
                return new HandleResult(false, $"User with email '{parameters.Email}' already exists");

            ulong uid = (ulong)new Random().Next(1, int.MaxValue);

            bool admin = db.SdkUsers.Count() == 0;

            db.SdkUsers.Add(new SdkUser()
            {
                ID = uid,
                Email = parameters.Email,
                PasswordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(parameters.Password))).ToLower(),
                RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                IsAdmin = admin,
                PlayerName = "Player_" + Rng.RandomString(8),
            });

            db.Users.Add(new GameUser()
            {
                ID = uid
            });
            db.SaveChanges();

            return new HandleResult(true, $"User '{parameters.Email}' registered successfully with ID {uid}");
        }
    }
}
