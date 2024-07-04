using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/get")]
    public class GetInventoryData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetInventoryData>();
            var user = GetUser();

            var response = new ResGetInventoryData();
            foreach (var item in user.Items)
            {
                response.Items.Add(new NetUserItemData() { Tid = item.ItemType, Count = 1 });
            }

            // TODO implement

            WriteData(response);
        }
    }
}
