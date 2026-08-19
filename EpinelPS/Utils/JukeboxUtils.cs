using EpinelPS.Data;
using EpinelPS.Models;

namespace EpinelPS.Utils;

public static class JukeboxUtils
{
    /// <summary>
    /// Songs available to the user: default BGMs plus everything unlocked
    /// (unlocks are granted through RewardType.Bgm rewards, e.g. field items).
    /// </summary>
    public static List<int> GetUnlockedSongs(User user)
    {
        var owned = new HashSet<int>(user.JukeboxBgm);
        var result = new List<int>();
        foreach (var song in GameData.Instance.jukeboxListDataRecords.Values)
        {
            if (song.IsDefaultBgm || owned.Contains(song.Id))
                result.Add(song.Id);
        }
        return result;
    }

    /// <summary>
    /// Number of songs of a theme the user owns (default BGMs included).
    /// </summary>
    public static int CountOwnedSongsByTheme(User user, int theme)
    {
        var owned = new HashSet<int>(user.JukeboxBgm);
        int count = 0;
        foreach (var song in GameData.Instance.jukeboxListDataRecords.Values)
        {
            if (song.Theme != theme)
                continue;
            if (song.IsDefaultBgm || owned.Contains(song.Id))
                count++;
        }
        return count;
    }

    /// <summary>
    /// Builds the client's view of the current BGM setting for a location.
    /// </summary>
    public static NetJukeboxBgm BuildCurrentBgm(User user, NetJukeboxLocation location)
    {
        var setting = location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
        var bgm = new NetJukeboxBgm { Location = location, Type = setting.Type, IsShuffle = setting.IsShuffle };
        switch (setting.Type)
        {
            case NetJukeboxBgmType.JukeboxPlaylist:
                var playlist = user.PlayLists.FirstOrDefault(p => p.JukeboxPlaylistUid == setting.TableId);
                if (playlist != null)
                {
                    bgm.JukeboxPlaylist = playlist;
                }
                else
                {
                    bgm.Type = NetJukeboxBgmType.JukeboxTableId;
                    bgm.JukeboxTableId = setting.TableId;
                }
                break;
            case NetJukeboxBgmType.JukeboxFavorite:
                bgm.JukeboxFavorite = user.FavoriteSongs;
                break;
            default:
                bgm.JukeboxTableId = setting.TableId;
                break;
        }
        return bgm;
    }
}
