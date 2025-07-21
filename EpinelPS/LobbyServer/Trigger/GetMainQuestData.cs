using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/Trigger/GetMainQuestData")]
    public class GetMainQuestData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMainQuestData req = await ReadData<ReqGetMainQuestData>();
            Database.User user = GetUser();

            ResGetMainQuestData response = new();
            foreach (KeyValuePair<int, bool> item in user.MainQuestData)
            {
                response.MainQuestList.Add(new NetMainQuestData() { IsReceived = item.Value, Tid = item.Key });
            }

            await WriteDataAsync(response);
        }
    }
}
