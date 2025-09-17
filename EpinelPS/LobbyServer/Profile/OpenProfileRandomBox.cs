using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Profile
{
    [PacketPath("/ProfileCard/ProfileRandomBox/Open")]
    public class OpenProfileRandomBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "isn": "3120203", "numOpens": 1 }
            ReqOpenProfileRandomBox req = await ReadData<ReqOpenProfileRandomBox>();
            User user = GetUser();

            ResOpenProfileRandomBox response = new();

            // find box in inventory
            ItemData box = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() ?? throw new InvalidDataException("cannot find box with isn " + req.Isn);
            if (req.NumOpens > box.Count) throw new Exception("count mismatch");

            box.Count -= req.NumOpens;
            if (box.Count == 0) user.Items.Remove(box);

            // update client side box count
            response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(box));

            // find matching probability entries
            RandomItemRecord[] entries = [.. GameData.Instance.RandomItem.Values.Where(x => x.group_id == box.ItemType)];
            if (entries.Length == 0) throw new Exception($"cannot find any probability entries with ID {box.ItemType}");

            for (int i = 0; i < req.NumOpens; i++)
            {
                RandomItemRecord winningRecord = Rng.PickWeightedItem([.. entries]);
                Logging.WriteLine($"LootBox {box.ItemType}: Won item - {JsonConvert.SerializeObject(winningRecord)}", LogType.Debug);

                if (winningRecord.reward_value_min != winningRecord.reward_value_max)
                {
                    Logging.WriteLine("TODO: reward_value_max", LogType.Warning);
                }

                if (winningRecord.reward_type == "ProfileCardObject")
                {

                    ItemData? existingItem = user.Items.Where(x => x.ItemType == winningRecord.reward_id).FirstOrDefault();
                    
                    // if user already has the item, convert to ticket material instead
                    if (existingItem != null)
                    {
                        // find ticket material ID from ProfileCardObjectTable
                        if (GameData.Instance.ProfileCardObjectTable.TryGetValue(winningRecord.reward_id, out ProfileCardObjectTableRecord? ProfileCardObjectData))
                        {
                            int ticketMaterialTid = ProfileCardObjectData.exchange_item_id;
                            ItemData? existingTicketMaterial = user.Items.Where(x => x.ItemType == ticketMaterialTid).FirstOrDefault();
                            if (existingTicketMaterial != null)
                            {
                                // add to existing item
                                existingTicketMaterial.Count += ProfileCardObjectData.exchange_item_value;
                            }
                            else
                            {
                                // create new item
                                ItemData newTicketMaterial = new()
                                {
                                    Isn = user.GenerateUniqueItemId(),
                                    ItemType = ticketMaterialTid,
                                    Count = ProfileCardObjectData.exchange_item_value
                                };
                                user.Items.Add(newTicketMaterial);
                                existingTicketMaterial = newTicketMaterial;
                            }
                            response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(existingTicketMaterial));
                        }
                        else
                        {
                            throw new Exception("cannot find ProfileCardObjectTable entry for item " + winningRecord.reward_id);
                        }
                        response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(existingItem));
                        response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult()
                        {
                            ObjectTid = existingItem.ItemType,
                            ExchangedForTicketMaterial = true
                        });
                        continue;
                    }

                    // otherwise give the item
                    ItemData newItem = new()
                    {
                        Isn = user.GenerateUniqueItemId(),
                        ItemType = winningRecord.reward_id
                    };
                    user.Items.Add(newItem);
                    response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(newItem));
                    response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult()
                    {
                        ObjectTid = newItem.ItemType
                    });
                }
                else
                {
                    Logging.WriteLine("TODO: handle reward type " + winningRecord.reward_type, LogType.Warning);
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}