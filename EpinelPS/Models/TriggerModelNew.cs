using System.ComponentModel.DataAnnotations.Schema;
using EpinelPS.Data;

namespace EpinelPS.Models;
public class TriggerModelNew
{
    public Trigger Type { get; set; }
    public long Id { get; set; }
    public long CreatedAt { get; set; }
    public int ConditionId { get; set; }
    public int Value { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }
    
    public NetTrigger ToNet()
    {
        return new()
        {
            ConditionId = ConditionId,
            CreatedAt = CreatedAt,
            Seq = Id,
            Trigger = (int)Type,
            UserValue = Value
        };
    }
}
