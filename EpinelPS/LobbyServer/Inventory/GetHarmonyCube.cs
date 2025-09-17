using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/getharmonycube")]
    public class GetHarmonyCube : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetHarmonyCube req = await ReadData<ReqGetHarmonyCube>();
            User user = GetUser();

            ResGetHarmonyCube response = new();
            List<ItemData> harmonyCubes = [.. user.Items.Where(item =>
                GameData.Instance.ItemHarmonyCubeTable.ContainsKey(item.ItemType))];

            foreach (ItemData harmonyCube in harmonyCubes)
            {
                if (GameData.Instance.ItemHarmonyCubeTable.TryGetValue(harmonyCube.ItemType, out ItemHarmonyCubeRecord? harmonyCubeData))
                {
                    NetUserHarmonyCubeData netHarmonyCube = new()
                    {
                        Isn = harmonyCube.Isn,
                        Tid = harmonyCube.ItemType,
                        Lv = harmonyCube.Level
                    };

                    foreach (long csn in harmonyCube.CsnList)
                    {
                        netHarmonyCube.CsnList.Add(csn);
                    }

                    if (harmonyCube.Csn > 0 && !harmonyCube.CsnList.Contains(harmonyCube.Csn))
                    {
                        netHarmonyCube.CsnList.Add(harmonyCube.Csn);
                    }

                    response.HarmonyCubes.Add(netHarmonyCube);
                }
            }


            await WriteDataAsync(response);
        }
    }
}
