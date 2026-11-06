
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class GameUser
{
    public ulong ID { get; set; }
    public string Nickname { get; set; } = "Player";
    public string Description { get; set; } = "";
    public DateTime LastAction { get; set;}
    public ICollection<TriggerModelNew> Triggers { get; set; } = new List<TriggerModelNew>();
    public ICollection<CharacterToggleModel> CharacterToggles { get; set; } = new List<CharacterToggleModel>();
}
