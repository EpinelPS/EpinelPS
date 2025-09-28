using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/clearequipment")]
    public class ClearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearEquipment req = await ReadData<ReqClearEquipment>();
            User user = GetUser();

            ResClearEquipment response = new();

            foreach (ItemData item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    // update character Id
                    item.Csn = 0;

                    response.Item = NetUtils.ToNet(item);
                    break;
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
