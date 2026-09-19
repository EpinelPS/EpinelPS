using System.Text;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class HelpParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.String(0, "commandName", "Name of the command to show detailed information for", true),
    ];

    public string CommandName { get; init; }
}

public class HelpHandler(IExecutionContext context, ICommandRegistry registry) : BaseHandler<HelpParameter>(context)
{
    public override string Name => "help";
    public override string Description => "Display help information for available commands";
    public override string[] Alias => ["h", "?"];

    protected async override Task<HandleResult> ExecuteAsync(HelpParameter parameters)
    {
        return parameters.CommandName is null
            ? ShowCommandList()
            : ShowCommandDetail(parameters.CommandName);
    }

    private HandleResult ShowCommandList()
    {
        var handlers = registry.GetHandlers();
        var padding = handlers.Max(cmd => cmd.Name.Length);
        var sb = new StringBuilder();
        sb.AppendLine("Available commands:");
        foreach (var cmd in handlers)
            sb.AppendLine($"  {cmd.Name.PadRight(padding)} - {cmd.Description}");
        sb.AppendLine();
        sb.Append("Use 'help <command>' for more details on a specific command.");

        return new HandleResult(true, sb.ToString());
    }

    private HandleResult ShowCommandDetail(string commandName)
    {
        var cmd = registry.FindHandler(commandName);
        if (cmd is null)
            return new HandleResult(false, $"Unknown command: '{commandName}'. Type 'help' to see available commands.");

        var sb = new StringBuilder();
        sb.AppendLine($"Command:\t{cmd.Name}");
        sb.AppendLine($"Description:\t{cmd.Description}");
        sb.AppendLine($"Usage:\t\t{cmd.Usage}");
        if (cmd.Alias.Length > 0)
            sb.AppendLine($"Aliases:\t{string.Join(", ", cmd.Alias)}");

        return new HandleResult(true, sb.ToString().TrimEnd());
    }
}
