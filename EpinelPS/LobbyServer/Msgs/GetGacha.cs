using EpinelPS.Utils;

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
            if (user.GachaTutorialPlayCount > 0)
                response.Gacha.Add(new NetUserGachaData() { GachaType = 3, PlayCount = 1 });
            await WriteDataAsync(response);
        }
    }
}
