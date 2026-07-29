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

        List<int> wishlist = req.DataList.Select(d => d.SlotList.FirstOrDefault(-1)).Where(id => id != -1).ToList();

        // If the list is empty, try to parse the body directly.
        if (wishlist.Count == 0)
        {
            ctx.Request.Body.Position = 0;
            try
            {
                var reqContent = StreamUtils.ParseNetCustomPackageSetupData(ctx.Request.Body);
                wishlist = reqContent.Select(l => l.SlotList[0]).ToList();
            }
            catch (Exception ex)
            {
                // Can't read the request, fail gracefully
                Logging.WriteLine("Could not read [ReqSetCustomPackageSlot]");
            }
        }

        // Parse the request directly if the request is empty

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

        // Save
        JsonDb.Save();

        await WriteDataAsync(req);
    }

}