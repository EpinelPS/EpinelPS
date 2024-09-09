using EpinelPS.Utils;
using EpinelPS.StaticInfo;

namespace EpinelPS.LobbyServer.Msgs
{
    [PacketPath("/Gacha/Get")]
    public class GetGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetGachaData>();
            var user = GetUser();

            var response = new ResGetGachaData();

            // Adding a default GachaType if the tutorial is done
            if (user.GachaTutorialPlayCount > 0)
            {
                response.Gacha.Add(new NetUserGachaData() { GachaType = 3, PlayCount = 1 });
            }

            // Now let's loop through gachaTypes and add those with "type" == "GachaPickup"
            foreach (var gacha in GameData.Instance.gachaTypes.Values)  // We're looping through the dictionary's values
            {
                if (gacha.type == "GachaPickup")
                {
                    // Add this GachaType ID to the response
                    response.Gacha.Add(new NetUserGachaData() { GachaType = gacha.id, PlayCount = 1 });
                }
            }

            // Write the response back
            await WriteDataAsync(response);
        }
    }
}
