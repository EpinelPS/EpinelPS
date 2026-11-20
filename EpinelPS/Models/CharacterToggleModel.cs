using System.ComponentModel.DataAnnotations.Schema;

namespace EpinelPS.Models;

/// <summary>
/// Categories of per-character display toggles used by the /character/toggle/* endpoints.
/// </summary>
public enum CharacterToggleType : int
{
    /// <summary>
    /// Toggles the character info panel
    /// </summary>
    CharacterInfo = 0,

    /// <summary>
    /// Toggles the favorite item wallpaper of the character
    /// </summary>
    FavoriteItemWallpaper = 1,

    /// <summary>
    /// Toggles the background of the character
    /// </summary>
    CharacterBackground = 2
}

/// <summary>
/// Stores a single character display toggle owned by a player.
/// Only toggles that the player explicitly changed are stored, missing rows mean
/// that the client should use its own default value.
/// </summary>
public class CharacterToggleModel
{
    public long Id { get; set; }
    public ulong UserId { get; set; }
    public int Csn { get; set; }
    public CharacterToggleType Type { get; set; }
    public bool Toggle { get; set; }

    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; } = null!;
}
