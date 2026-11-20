using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Toggle;

/// <summary>
/// Shared logic for the /character/toggle/* endpoints.
/// Only toggles that the player explicitly changed are stored, characters without a
/// stored row are left to the client default value.
/// </summary>
internal static class CharacterToggleService
{
    /// <summary>
    /// Stores a display toggle for a character that is owned by the player.
    /// Csn values that are out of range or that do not belong to the player are ignored.
    /// </summary>
    internal static async Task SetAsync(GameContext context, User user, CharacterToggleType type, long csn, bool toggle)
    {
        if (csn <= 0 || csn > int.MaxValue)
        {
            Logging.WriteLine($"[CharacterToggle] ignoring out of range Csn {csn}", LogType.Debug);
            return;
        }

        int characterCsn = (int)csn;
        if (!user.Characters.Any(x => x.Csn == characterCsn))
        {
            Logging.WriteLine($"[CharacterToggle] ignoring Csn {characterCsn}, not owned by user {user.ID}", LogType.Debug);
            return;
        }

        CharacterToggleModel? existing = context.CharacterToggles
            .FirstOrDefault(x => x.UserId == user.ID && x.Csn == characterCsn && x.Type == type);

        if (existing == null)
        {
            context.CharacterToggles.Add(new CharacterToggleModel()
            {
                UserId = user.ID,
                Csn = characterCsn,
                Type = type,
                Toggle = toggle
            });
        }
        else
        {
            existing.Toggle = toggle;
        }

        await context.SaveChangesAsync();

        Logging.WriteLine($"[CharacterToggle] user {user.ID} set {type} of character {characterCsn} to {toggle}", LogType.Debug);
    }

    /// <summary>
    /// Sorts the stored toggles into the three response lists of ResListCharacterToggle.
    /// </summary>
    internal static void FillList(ResListCharacterToggle response, IEnumerable<CharacterToggleModel> toggles)
    {
        foreach (CharacterToggleModel toggle in toggles)
        {
            NetCharacterToggleInfo info = new()
            {
                Csn = toggle.Csn,
                Toggle = toggle.Toggle
            };

            switch (toggle.Type)
            {
                case CharacterToggleType.CharacterInfo:
                    response.CharacterInfoToggles.Add(info);
                    break;

                case CharacterToggleType.FavoriteItemWallpaper:
                    response.FavoriteItemWallpaperToggles.Add(info);
                    break;

                case CharacterToggleType.CharacterBackground:
                    response.CharacterBackgroundToggles.Add(info);
                    break;
            }
        }
    }
}

/// Path: /character/toggle/characterinfo
[GameRequest("/character/toggle/characterinfo")]
public class ToggleCharacterInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqToggleCharacterInfo req = await ReadData<ReqToggleCharacterInfo>();
        User user = GetUser();

        await CharacterToggleService.SetAsync(GameContext, user, CharacterToggleType.CharacterInfo, req.Csn, req.Toggle);

        ResToggleCharacterInfo response = new();
        await WriteDataAsync(response);
    }
}
/// Path: /character/toggle/favoriteitemwallpaper
[GameRequest("/character/toggle/favoriteitemwallpaper")]
public class ToggleFavoriteItemWallpaper : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqToggleCharacterFavoriteItemWallpaper req = await ReadData<ReqToggleCharacterFavoriteItemWallpaper>();
        User user = GetUser();

        await CharacterToggleService.SetAsync(GameContext, user, CharacterToggleType.FavoriteItemWallpaper, req.Csn, req.Toggle);

        ResToggleCharacterFavoriteItemWallpaper response = new();
        await WriteDataAsync(response);
    }
}
/// Path: /character/toggle/characterbackground
[GameRequest("/character/toggle/characterbackground")]
public class ToggleCharacterBackground : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqToggleCharacterBackground req = await ReadData<ReqToggleCharacterBackground>();
        User user = GetUser();

        await CharacterToggleService.SetAsync(GameContext, user, CharacterToggleType.CharacterBackground, req.Csn, req.Toggle);

        ResToggleCharacterBackground response = new();
        await WriteDataAsync(response);
    }
}
/// Path: /character/toggle/list
[GameRequest("/character/toggle/list")]
public class ToggleList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqListCharacterToggle>();
        User user = GetUser();

        ResListCharacterToggle response = new();
        CharacterToggleService.FillList(response, GameContext.CharacterToggles.Where(x => x.UserId == user.ID));
        await WriteDataAsync(response);
    }
}


