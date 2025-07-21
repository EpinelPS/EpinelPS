using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/list")]
    public class ListSubquests : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetSubQuestList req = await ReadData<ReqGetSubQuestList>();
            User user = GetUser();

            ResGetSubQuestList response = new();

            foreach(KeyValuePair<int, bool> item in user.SubQuestData)
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
