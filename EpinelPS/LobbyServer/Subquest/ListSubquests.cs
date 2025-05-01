using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/list")]
    public class ListSubquests : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSubQuestList>();
            var user = GetUser();

            var response = new ResGetSubQuestList();

            foreach(var item in user.SubQuestData)
            {
                response.SubquestList.Add(new NetSubQuestData(){
                    CreatedAt = DateTime.UtcNow.Ticks, // TODO does this matter
                    SubQuestId = item.Key,
                    IsReceived = item.Value
                });
            }

            await WriteDataAsync(response);
        }
    }
}
