using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class AddLocaleFilesParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.String(0, "localedir", "Folder that has decrypted locale files"),
    ];

    public string LocaleDir { get; init; }
}

public class AddLocaleFilesHandler(IExecutionContext context) : BaseHandler<AddLocaleFilesParameter>(context)
{
    public override string Name => "add-locale";
    public override string Description => "Select path that has decrypted locale files";

    protected async override Task<HandleResult> ExecuteAsync(AddLocaleFilesParameter parameters)
    {
        var rsp = AdminCommands.AddLocaleFiles(parameters.LocaleDir);
        JsonDb.Save();
        return rsp.ok
            ? new HandleResult(true, $"Locale Files added successfully")
            : new HandleResult(false, rsp.error);
    }
}
