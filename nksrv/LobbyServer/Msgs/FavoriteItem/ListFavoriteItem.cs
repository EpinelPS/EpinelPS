using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.FavoriteItem
{
    [PacketPath("/favoriteitem/list")]
    public class ListFavoriteItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListFavoriteItem>();
            var user = GetUser();

            var response = new ResListFavoriteItem();

            await WriteDataAsync(response);
        }
    }
}
