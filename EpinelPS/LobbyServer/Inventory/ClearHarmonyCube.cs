using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/clearharmonycube")]
    public class ClearHarmonyCube : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearHarmonyCube req = await ReadData<ReqClearHarmonyCube>();
            User user = GetUser();


            ResClearHarmonyCube response = new();

            foreach (ItemData item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    if (req.Csn > 0)
                    {
                        if (item.CsnList.Contains(req.Csn))
                        {
                            item.CsnList.Remove(req.Csn);
                        }

                        if (item.CsnList.Count > 0)
                        {
                            item.Csn = item.CsnList[0];
                            if (GameData.Instance.ItemHarmonyCubeTable.TryGetValue(item.ItemType, out var harmonyCubeData))
                            {
                                item.Position = harmonyCubeData.location_id;
                            }
                        }
                        else
                        {
                            item.Csn = 0;
                            item.Position = 0;
                        }
                    }
                    else
                    {
                        item.CsnList.Clear();
                        item.Csn = 0;
                        item.Position = 0;
                    }

                    NetUserHarmonyCubeData netHarmonyCube = new()
                    {
                        Isn = item.Isn,
                        Tid = item.ItemType,
                        Lv = item.Level
                    };

                    foreach (long csn in item.CsnList)
                    {
                        netHarmonyCube.CsnList.Add(csn);
                    }

                    if (item.Csn > 0 && !item.CsnList.Contains(item.Csn))
                    {
                        netHarmonyCube.CsnList.Add(item.Csn);
                    }

                    response.HarmonyCube = netHarmonyCube;
                    break;
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
