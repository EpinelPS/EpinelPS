using EpinelPS.Database;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class ReloadDbParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [];
}

public class ReloadDbHandler(IExecutionContext context) : BaseHandler<ReloadDbParameter>(context)
{
    public override string Name => "reload-db";
    public override string Description => "Reload the database from the JSON file";
    public override string[] Alias => ["reload", "r"];

    protected async override Task<HandleResult> ExecuteAsync(ReloadDbParameter parameters)
    {
        JsonDb.Reload();
        return new HandleResult(true, "Database reloaded successfully");
    }
}
