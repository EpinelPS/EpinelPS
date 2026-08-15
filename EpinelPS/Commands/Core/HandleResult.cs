namespace EpinelPS.Commands.Core;

public readonly struct HandleResult(bool result, string? message = null, ConsoleColor? color = null)
{
    public bool Result { get; } = result;
    public string? Message { get; } = message;
    public ConsoleColor? Color { get; } = color;
}
