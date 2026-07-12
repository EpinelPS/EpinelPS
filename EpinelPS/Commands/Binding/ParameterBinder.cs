using System.Reflection;
using EpinelPS.Commands.Core;

namespace EpinelPS.Commands.Binding;

/// <summary>
/// Binds raw string arguments to a strongly-typed parameters object
/// according to a set of <see cref="ParameterDescriptor"/>.
/// </summary>
public static class ParameterBinder
{
    /// <summary>
    /// Parse <paramref name="args"/> into an instance of <typeparamref name="T"/>
    /// using the given <paramref name="descriptors"/>.
    /// </summary>
    public static BindingResult<T> Bind<T>(string[] args, ParameterDescriptor[] descriptors)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(descriptors);

        if (descriptors.Length == 0)
            return BindingResult<T>.Success(new T());

        var ordered = descriptors.OrderBy(d => d.Position).ToArray();

        // --- Step 1: verify minimum argument count ---
        var lastRequired = ordered
            .Where(d => !d.IsOptional)
            .Select(d => (int?)d.Position)
            .LastOrDefault();

        if (lastRequired.HasValue && args.Length <= lastRequired.Value)
        {
            var expected = lastRequired.Value + 1;
            return BindingResult<T>.Failure(
                $"Insufficient arguments: expected at least {expected}, got {args.Length}");
        }

        // --- Step 2: convert and validate each descriptor ---
        var resolved = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var allErrors = new List<string>();

        foreach (var desc in ordered)
        {
            object? value;

            if (desc.Position >= args.Length)
            {
                // Argument not supplied — must be optional
                if (!desc.IsOptional)
                {
                    allErrors.Add($"Required parameter '{desc.Name}' at position {desc.Position} is missing");
                    continue;
                }
                value = desc.DefaultValue;
            }
            else
            {
                // Convert
                var raw = args[desc.Position];
                var (converted, convError) = desc.Converter(raw);
                if (convError != null)
                {
                    allErrors.Add(convError);
                    continue;
                }
                value = converted;

                // Validate constraints
                foreach (var constraint in desc.Constraints)
                {
                    var cError = constraint.Validate(value!, desc.Name);
                    if (cError != null)
                    {
                        allErrors.Add(cError);
                        break; // one error per parameter is enough
                    }
                }
            }

            resolved[desc.Name] = value;
        }

        if (allErrors.Count > 0)
            return BindingResult<T>.Failure(string.Join("; ", allErrors));

        // --- Step 3: materialise T and set properties ---
        var instance = new T();
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

        foreach (var (name, value) in resolved)
        {
            var prop = Array.Find(props, p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (prop == null)
            {
                // Descriptor name doesn't match any property — not fatal,
                // but indicates a likely programming error.
                continue;
            }

            if (value is not null && !prop.PropertyType.IsInstanceOfType(value))
            {
                return BindingResult<T>.Failure(
                    $"Type mismatch for '{name}': expected {prop.PropertyType.Name}, got {value.GetType().Name}");
            }

            prop.SetValue(instance, value);
        }

        return BindingResult<T>.Success(instance);
    }
}
