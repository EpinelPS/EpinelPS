using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/userandombox")]
    public class UseRandomBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqUseRandomBox req = await ReadData<ReqUseRandomBox>();
            User user = GetUser();

            ResUseRandomBox response = new();

            ItemData box = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() ?? throw new InvalidDataException("cannot find box with isn " + req.Isn);
            if (req.Count > box.Count) throw new Exception("count mismatch");

            box.Count -= req.Count;
            if (box.Count == 0) user.Items.Remove(box);

            response.Reward = NetUtils.UseLootBox(user, box.ItemType, req.Count);

            // update client side box count
            response.Reward.UserItems.Add(NetUtils.UserItemDataToNet(box));

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
