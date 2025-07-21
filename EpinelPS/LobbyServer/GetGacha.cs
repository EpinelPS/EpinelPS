using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer
{
    [PacketPath("/Gacha/Get")]
    public class GetGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetGachaData req = await ReadData<ReqGetGachaData>();
            Database.User user = GetUser();

            ResGetGachaData response = new();

            // TODO: should not return anything when not completed chatper 2

            // Adding a default GachaType if the tutorial is done
            if (user.GachaTutorialPlayCount > 0)
            {
                response.Gacha.Add(new NetUserGachaData() { GachaType = 3, PlayCount = 1 });
            }

			response.Gacha.Add(new NetUserGachaData() { GachaType = 9, PlayCount = 0 }); //type 9 = pickup gacha
			response.GachaEventData.Add(new NetGachaEvent() {FreeCount = 1, GachaTypeId = 9  } ); 
			response.MultipleCustom.Add(new NetGachaCustomData() {Type = 9, Tid = 451101});
            // Write the response back
            await WriteDataAsync(response);
        }
    }
}
