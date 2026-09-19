using System.Reflection;
using EpinelPS.Commands.Core;

namespace EpinelPS.Commands.Services;

public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, Func<IExecutionContext, IHandler>> handlerFactories = [];
    private readonly List<IHandlerInfo> handlerInfos = [];

    public CommandRegistry()
    {
        RegisterHandlers();
    }

    public IReadOnlyList<IHandlerInfo> GetHandlers() => handlerInfos;

    public IHandlerInfo? FindHandler(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return handlerInfos.FirstOrDefault(h =>
            h.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            h.Alias.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    public IHandler? CreateHandler(string name, IExecutionContext context)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var key = name.ToLowerInvariant();
        if (handlerFactories.TryGetValue(key, out var factory))
            return factory(context);

        return null;
    }

    private void RegisterHandlers()
    {
        var baseHandler = typeof(BaseHandler<>);
        var handlerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
            .Where(t => InheritsFromGenericBase(t, baseHandler));

        foreach (var type in handlerTypes)
        {
            try
            {
                RegisterHandler(type);
            }
            catch
            {
                // Skip handlers that fail to register
            }
        }
    }

    private void RegisterHandler(Type type)
    {
        // Create a temporary instance to read metadata
        IHandlerInfo info;
        if (type == typeof(Handler.HelpHandler))
        {
            // HelpHandler needs ICommandRegistry as second constructor parameter
            info = (IHandlerInfo)Activator.CreateInstance(type, new CliContext(), this)!;
        }
        else
        {
            info = (IHandlerInfo)Activator.CreateInstance(type, new CliContext())!;
        }

        // Register factory
        IHandler factory(IExecutionContext ctx)
        {
            if (type == typeof(Handler.HelpHandler))
                return (IHandler)Activator.CreateInstance(type, ctx, this)!;
            return (IHandler)Activator.CreateInstance(type, ctx)!;
        }

        // Register by name
        var key = info.Name.ToLowerInvariant();
        if (!handlerFactories.ContainsKey(key))
        {
            handlerFactories[key] = factory;
        }

        // Register by aliases
        foreach (var alias in info.Alias)
        {
            var aliasKey = alias.ToLowerInvariant();
            if (!handlerFactories.ContainsKey(aliasKey))
            {
                handlerFactories[aliasKey] = factory;
            }
        }

        handlerInfos.Add(info);
    }

    private static bool InheritsFromGenericBase(Type type, Type genericBase)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase)
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
