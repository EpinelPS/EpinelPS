using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Core;

public abstract class BaseHandler<T>(IExecutionContext context) : IHandler
    where T : ICommandParameters, new()
{
    protected readonly IExecutionContext context = context;
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual string[] Alias => [];
    public string Usage
    {
        get
        {
            var ordered = T.Descriptors.OrderBy(d => d.Position);
            var parts = ordered.Select(d =>
                d.IsOptional ? $"[{d.Name}]" : $"<{d.Name}>");
            return string.Join(" ", new[] { Name }.Concat(parts));
        }
    }

    public async Task<HandleResult> ExecuteAsync(string[] args)
    {
        var result = ParameterBinder.Bind<T>(args, T.Descriptors);
        if (!result.IsSuccess)
            return new HandleResult(false, result.Error);
        return await ExecuteAsync(result.Value!);
    }

    protected abstract Task<HandleResult> ExecuteAsync(T parameters);
}
