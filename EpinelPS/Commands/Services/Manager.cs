using EpinelPS.Commands.Core;

namespace EpinelPS.Commands.Services;

public class Manager : ICommandRegistry
{
    private readonly CommandRegistry registry;
    private readonly CliContext context = new();

    public Manager()
    {
        registry = new CommandRegistry();
        var handlers = registry.GetHandlers();
        Console.WriteLine($"[Manager] Registered {handlers.Count} handler(s)");
    }

    public string? UserName => context.SelectedUser?.Username;

    public IReadOnlyList<IHandlerInfo> GetHandlers() => registry.GetHandlers();

    public IHandlerInfo? FindHandler(string name) => registry.FindHandler(name);

    public async Task<HandleResult> ExecuteCommandAsync(string input)
    {
        var parts = ParseInput(input);
        if (parts.Length == 0)
            return new HandleResult(false, "Type 'help' to see available commands.");

        var commandName = parts[0].ToLower();

        var handler = registry.CreateHandler(commandName, context);
        if (handler != null)
            return await handler.ExecuteAsync(parts[1..]);

        return new HandleResult(false, $"Unknown command: '{input}'.\nType 'help' to see available commands.");
    }

    private static string[] ParseInput(string input)
    {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
