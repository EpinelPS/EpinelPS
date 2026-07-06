
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class GameUser
{
    public ulong ID { get; set; }
    public string Nickname { get; set; } = "Player";
    public DateTime LastAction { get; set;}
    public ICollection<TriggerModelNew> Triggers { get; set; } = new List<TriggerModelNew>();
}
