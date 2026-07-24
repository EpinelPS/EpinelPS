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
        var req = await ReadData<ReqSetCustomPackageSlot>();

        // Add the characters to the user wishlist. For now, I will assume that the game sent the right data.
        // TODO: Check that all ids are correct and only from the allowed pull list
         User user = GetUser();

        var wishlist = req.DataList.Select(d => d.SlotList.FirstOrDefault(-1)).Where(id => id != -1).ToList();

        // The wishlist must have 20 characters to be valid, otherwise clear it.
        if (wishlist.Count == 20)
        {
            user.CharacterWishlist = wishlist;
        }
        else
        {
            user.CharacterWishlist.Clear();
        }        

        Logging.WriteLine($"[SetCustomWishlistSlot] Updated wishlist for user {user.ID}: [{string.Join(", ", user.CharacterWishlist)}]");


        await WriteDataAsync(req);
    }
    
}