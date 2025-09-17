using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Profile
{
    [PacketPath("/ProfileCard/Buy")]
    public class BuyProfileCard : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBuyProfileCardObject req = await ReadData<ReqBuyProfileCardObject>();
            User user = GetUser();

            ResBuyProfileCardObject response = new();

            if (user.Items.Count > 0)
            {
                ItemData? item = user.Items.FirstOrDefault(i => i.ItemType == req.ObjectTid);
                long isn = user.GenerateUniqueItemId();
                response.ProfileCardTicketMaterialSync = new NetUserItemData
                {
                    Isn = isn,
                    Tid = req.ObjectTid,
                    Lv = 0,
                    Exp = 0,
                    Count = 0
                };
                if (item == null)
                {
                    user.Items.Add(new ItemData
                    {
                        Isn = isn,
                        ItemType = req.ObjectTid,
                        Level = 0,
                        Exp = 0,
                        Count = 0
                    });
                }
                else
                {
                    item.Csn = 0;
                    item.Level = 0;
                    item.Exp = 0;
                    item.Count = 0;
                    response.ProfileCardTicketMaterialSync.Csn = 0;
                }
                JsonDb.Save();
            }

            await WriteDataAsync(response);

        }
    }
}