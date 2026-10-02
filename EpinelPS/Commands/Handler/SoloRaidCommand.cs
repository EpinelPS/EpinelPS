using System.Text;
using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.LobbyServer.Soloraid;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class SoloRaidParameter : ICommandParameters
{
    public static ParameterDescriptor[] Descriptors => [
        Param.String(0, "action", "Action: info, list, mode, set", isOptional: true),
        Param.String(1, "value", "Value: mode name (AutoCycle, Latest, Fixed) or raid ID", isOptional: true),
    ];

    public string Action { get; init; } = "info";
    public string? Value { get; init; }
}

public class SoloRaidHandler(IExecutionContext context) : BaseHandler<SoloRaidParameter>(context)
{
    public override string Name => "soloraid";
    public override string Description => "Manage Solo Raid boss selection and rotation (info, list, mode <AutoCycle|Latest|Fixed>, set <id>)";
    public override string[] Alias => ["sr"];

    protected override async Task<HandleResult> ExecuteAsync(SoloRaidParameter parameters)
    {
        string action = (parameters.Action ?? "info").ToLowerInvariant();
        var validRaids = SoloRaidHelper.GetValidRaidIds();

        if (action == "info")
        {
            int currentId = SoloRaidHelper.GetRaidId();
            string bossName = SoloRaidHelper.GetBossName(currentId);
            string mode = GameConfig.Root.SoloRaidMode ?? "Latest";

            var sb = new StringBuilder();
            sb.AppendLine("=== Solo Raid Status ===");
            sb.AppendLine($"Current Mode    : {mode}");
            sb.AppendLine($"Active Boss ID  : {currentId}");
            sb.AppendLine($"Active Boss Name: {bossName}");
            sb.AppendLine($"Total Raids     : {validRaids.Count}");
            if (mode.Equals("AutoCycle", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine("Rotation        : Weekly automatic rotation across all valid bosses");
            else if (mode.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine($"Fixed ID        : {GameConfig.Root.SoloRaidFixedId}");

            return new HandleResult(true, sb.ToString().TrimEnd());
        }

        if (action == "list")
        {
            int currentId = SoloRaidHelper.GetRaidId();
            var sb = new StringBuilder();
            sb.AppendLine("=== Available Solo Raid Bosses ===");
            foreach (int raidId in validRaids)
            {
                string marker = (raidId == currentId) ? " [ACTIVE]" : "";
                string bossName = SoloRaidHelper.GetBossName(raidId);
                sb.AppendLine($"  ID {raidId,4}: {bossName}{marker}");
            }
            return new HandleResult(true, sb.ToString().TrimEnd());
        }

        if (action == "mode")
        {
            string? modeValue = parameters.Value;
            if (string.IsNullOrWhiteSpace(modeValue))
                return new HandleResult(false, "Usage: soloraid mode <AutoCycle|Latest|Fixed>");

            string normalizedMode;
            if (modeValue.Equals("autocycle", StringComparison.OrdinalIgnoreCase) || modeValue.Equals("auto", StringComparison.OrdinalIgnoreCase))
                normalizedMode = "AutoCycle";
            else if (modeValue.Equals("latest", StringComparison.OrdinalIgnoreCase))
                normalizedMode = "Latest";
            else if (modeValue.Equals("fixed", StringComparison.OrdinalIgnoreCase))
                normalizedMode = "Fixed";
            else
                return new HandleResult(false, $"Unknown mode '{modeValue}'. Valid modes: AutoCycle, Latest, Fixed");

            GameConfig.Root.SoloRaidMode = normalizedMode;
            GameConfig.Save();

            int newId = SoloRaidHelper.GetRaidId();
            string bossName = SoloRaidHelper.GetBossName(newId);
            return new HandleResult(true, $"Solo Raid mode set to '{normalizedMode}'. Active boss: ID {newId} ({bossName})");
        }

        if (action == "set")
        {
            if (string.IsNullOrWhiteSpace(parameters.Value) || !int.TryParse(parameters.Value, out int targetId))
                return new HandleResult(false, "Usage: soloraid set <raidId>");

            if (!validRaids.Contains(targetId))
                return new HandleResult(false, $"Raid ID {targetId} not found in valid Solo Raid table. Type 'soloraid list' to view valid IDs.");

            GameConfig.Root.SoloRaidMode = "Fixed";
            GameConfig.Root.SoloRaidFixedId = targetId;
            GameConfig.Save();

            string bossName = SoloRaidHelper.GetBossName(targetId);
            return new HandleResult(true, $"Solo Raid successfully set to ID {targetId} ({bossName}) [Fixed mode]");
        }

        return new HandleResult(false, $"Unknown action '{parameters.Action}'. Use: info, list, mode, set");
    }
}
