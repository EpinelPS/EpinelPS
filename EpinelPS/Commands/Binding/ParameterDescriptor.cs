namespace EpinelPS.Commands.Binding;

/// <summary>
/// Describes a single command-line parameter: its name, type, position,
/// converter, constraints, and whether it is optional.
///
/// Descriptors are declared statically as an array by each command's parameter type
/// and consumed by <see cref="ParameterBinder"/> at runtime.
/// </summary>
public sealed record ParameterDescriptor
{
    public int Position { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Type ParameterType { get; init; } = null!;
    public bool IsOptional { get; init; }
    public object? DefaultValue { get; init; }
    public Func<string, (object? Value, string? Error)> Converter { get; init; } = null!;
    public IReadOnlyList<ParameterConstraint> Constraints { get; init; } = [];
}

/// <summary>
/// Factory helpers for creating <see cref="ParameterDescriptor"/> instances concisely.
/// </summary>
public static class Param
{
    public static ParameterDescriptor Int(
        int position, string name, string description, bool isOptional = false)
    {
        return new ParameterDescriptor
        {
            Position = position,
            Name = name,
            Description = description,
            ParameterType = typeof(int),
            IsOptional = isOptional,
            Converter = s => int.TryParse(s, out var v)
                ? ((object?)v, null)
                : (null, $"'{name}' must be an integer, got '{s}'"),
        };
    }

    public static ParameterDescriptor ULong(
        int position, string name, string description, bool isOptional = false)
    {
        return new ParameterDescriptor
        {
            Position = position,
            Name = name,
            Description = description,
            ParameterType = typeof(ulong),
            IsOptional = isOptional,
            Converter = s => ulong.TryParse(s, out var v)
                ? ((object?)v, null)
                : (null, $"'{name}' must be a non-negative integer, got '{s}'"),
        };
    }

    public static ParameterDescriptor Float(
        int position, string name, string description, bool isOptional = false)
    {
        return new ParameterDescriptor
        {
            Position = position,
            Name = name,
            Description = description,
            ParameterType = typeof(float),
            IsOptional = isOptional,
            Converter = s => float.TryParse(s, out var v)
                ? ((object?)v, null)
                : (null, $"'{name}' must be a number, got '{s}'"),
        };
    }

    public static ParameterDescriptor String(
        int position, string name, string description, bool isOptional = false)
    {
        return new ParameterDescriptor
        {
            Position = position,
            Name = name,
            Description = description,
            ParameterType = typeof(string),
            IsOptional = isOptional,
            Converter = s => ((object?)s, null),
        };
    }

    public static ParameterDescriptor Bool(
        int position, string name, string description, bool isOptional = false)
    {
        return new ParameterDescriptor
        {
            Position = position,
            Name = name,
            Description = description,
            ParameterType = typeof(bool),
            IsOptional = isOptional,
            Converter = s =>
            {
                if (bool.TryParse(s, out var v))
                    return ((object?)v, null);
                return (null, $"'{name}' must be 'true' or 'false', got '{s}'");
            },
        };
    }
}

/// <summary>
/// Extension methods for attaching constraints to a <see cref="ParameterDescriptor"/>.
/// </summary>
public static class ParameterDescriptorExtensions
{
    public static ParameterDescriptor WithConstraint(
        this ParameterDescriptor descriptor, ParameterConstraint constraint)
    {
        return descriptor with
        {
            Constraints = [..descriptor.Constraints, constraint],
        };
    }
}
