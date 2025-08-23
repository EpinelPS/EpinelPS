using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/equip")]
    public class EquipFavoriteItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEquipFavoriteItem req = await ReadData<ReqEquipFavoriteItem>();
            User user = GetUser();

            ResEquipFavoriteItem response = new();

            NetUserFavoriteItemData? favoriteItemToEquip = user.FavoriteItems.FirstOrDefault(f => f.FavoriteItemId == req.FavoriteItemId);
            if (favoriteItemToEquip == null)
            {
                throw new BadHttpRequestException($"FavoriteItem with ID {req.FavoriteItemId} not found", 404);
            }

            CharacterModel? character = user.Characters.FirstOrDefault(c => c.Csn == req.Csn);
            if (character == null)
            {
                throw new BadHttpRequestException($"Character with CSN {req.Csn} not found", 404);
            }

            NetUserFavoriteItemData? existingEquippedItem = user.FavoriteItems.FirstOrDefault(f => f.Csn == req.Csn);
            if (existingEquippedItem != null)
            {
                existingEquippedItem.Csn = 0; // Unequip
            }

            favoriteItemToEquip.Csn = req.Csn;

            foreach (NetUserFavoriteItemData item in user.FavoriteItems)
            {
                response.FavoriteItems.Add(item);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
