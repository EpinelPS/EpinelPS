using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Paseto;
using System.IO;

namespace EpinelPS.LobbyServer.Gacha;

/// <summary>
/// Handles the player wishlist choice updates.
/// </summary>
[GameRequest("/Gacha/SetCustom")]
public class SetCustomWishlistSlot : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSetGachaCustom>();

        // Add the characters to the user wishlist. For now, I will assume that the game sent the right data.
        User user = GetUser();
        List<NetGachaCustomData> wishlist = req.Custom.ToList();

        // Even though wishlisting is currently supported only on banner 1, since the game supports more than one banner,
        // let's delete the characters related to the wishlist banner ids specified in the request exclusively,
        // just in case the game later makes use of the other banner ids
        //
        // We also now save the list regardless of the amount of character selected. The amount of characters wislisted is checked when pulling

        user.CharacterWishlist.RemoveAll(c => wishlist.Select(w => w.Type).Distinct().Contains(c.BannerId));
        user.CharacterWishlist = wishlist.Select(w => new CharacterWishlistData() { BannerId = w.Type, CharacterId = w.Tid }).ToList();

        Logging.WriteLine($"[SetCustomWishlistSlot] Updated wishlist for user {user.ID}: [{string.Join(", ", user.CharacterWishlist)}]");

        // Save
        JsonDb.Save();

        await WriteDataAsync(req);
    }

}