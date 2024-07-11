using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Trigger
{
    [PacketPath("/Trigger/GetMainQuestData")]
    public class GetMainQuestData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMainQuestData>();
            var user = GetUser();

            var response = new ResGetMainQuestData();
            foreach (var item in user.MainQuestData)
            {
                response.MainQuestList.Add(new NetMainQuestData() { IsReceived = item.Value, Tid = item.Key });
            }
     
          await  WriteDataAsync(response);
        }
    }
}
