using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class UpdateServerParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class UpdateServerHandler(IExecutionContext context) : BaseHandler<UpdateServerParameter>(context)
{
    public override string Name => "update-server";
    public override string Description => "Update the game server's static data and resource information";
    public override string[] Alias => ["updateServer"];

    protected async override Task<HandleResult> ExecuteAsync(UpdateServerParameter parameters)
    {
        var rsp = await AdminCommands.UpdateResources();
        return rsp.ok
            ? new HandleResult(true, "Server data updated successfully")
            : new HandleResult(false, rsp.error);
    }
}
