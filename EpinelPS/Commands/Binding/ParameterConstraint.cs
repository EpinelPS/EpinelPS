namespace EpinelPS.Commands.Binding;

public abstract class ParameterConstraint
{
    /// <summary>
    /// Validate the converted value.
    /// </summary>
    /// <param name="value">The CLR value after type conversion</param>
    /// <param name="name">Parameter name (for error messages)</param>
    /// <returns>null if valid, or an error description</returns>
    public abstract string? Validate(object value, string name);
}

public sealed class IntRangeConstraint(int minimum, int maximum) : ParameterConstraint
{
    public int Minimum { get; } = minimum;
    public int Maximum { get; } = maximum;

    public override string? Validate(object value, string name)
    {
        var v = (int)value;
        if (v < Minimum || v > Maximum)
            return $"'{name}' must be between {Minimum} and {Maximum}, got {v}";
        return null;
    }
}

public sealed class ULongRangeConstraint(ulong minimum, ulong maximum) : ParameterConstraint
{
    public ulong Minimum { get; } = minimum;
    public ulong Maximum { get; } = maximum;

    public override string? Validate(object value, string name)
    {
        var v = (ulong)value;
        if (v < Minimum || v > Maximum)
            return $"'{name}' must be between {Minimum} and {Maximum}";
        return null;
    }
}

public sealed class FloatRangeConstraint(float minimum, float maximum) : ParameterConstraint
{
    public float Minimum { get; } = minimum;
    public float Maximum { get; } = maximum;

    public override string? Validate(object value, string name)
    {
        var v = (float)value;
        if (v < Minimum || v > Maximum)
            return $"'{name}' must be between {Minimum} and {Maximum}";
        return null;
    }
}

public sealed class StringLengthConstraint(int minimum, int maximum) : ParameterConstraint
{
    public int Minimum { get; } = minimum;
    public int Maximum { get; } = maximum;

    public override string? Validate(object value, string name)
    {
        var v = (string)value;
        if (v.Length < Minimum || v.Length > Maximum)
            return $"'{name}' length must be between {Minimum} and {Maximum}, got {v.Length}";
        return null;
    }
}

public sealed class PredicateConstraint(Func<object, string?> predicate) : ParameterConstraint
{
    public Func<object, string?> Predicate { get; } = predicate
        ?? throw new ArgumentNullException(nameof(predicate));

    public override string? Validate(object value, string name) => Predicate(value);
}
