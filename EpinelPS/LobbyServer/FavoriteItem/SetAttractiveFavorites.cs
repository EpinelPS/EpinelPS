using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/character/attractive/setfavorites")]
    public class SetAttractiveFavorites : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetAttractiveFavorites req = await ReadData<ReqSetAttractiveFavorites>();
            User user = GetUser();

            NetUserAttractiveData response = new();

            // Add all user's favorite items to the response
            foreach (NetUserAttractiveData item in user.BondInfo)
            {
                if (item.NameCode == req.NameCode)
                {
                    response = item;
                    if (req.IsFavorites)
                    {
                        response.IsFavorites = true;
                    }
                    else
                    {
                        response.IsFavorites = false;
                    }
                    JsonDb.Save();
                }
            }

            await WriteDataAsync(response);
        }
    }
}