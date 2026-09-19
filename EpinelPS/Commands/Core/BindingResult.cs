namespace EpinelPS.Commands.Core;

/// <summary>
/// Represents the outcome of binding raw string arguments to a strongly-typed parameters object.
/// </summary>
/// <typeparam name="T">The parameters type</typeparam>
public readonly struct BindingResult<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    private BindingResult(T value)
    {
        Value = value;
        Error = null;
    }

    private BindingResult(string error)
    {
        Value = default;
        Error = error;
    }

    public static BindingResult<T> Success(T value) => new(value);
    public static BindingResult<T> Failure(string error) => new(error);
}
