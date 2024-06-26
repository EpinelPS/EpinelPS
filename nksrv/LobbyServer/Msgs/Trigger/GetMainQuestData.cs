using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Trigger
{
    [PacketPath("/Trigger/GetMainQuestData")]
    public class GetMainQuestData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMainQuestData>();

            var response = new ResGetMainQuestData();
            response.MainQuestList.Add(new NetMainQuestData() { IsReceived = true, Tid = 1 });
            WriteData(response);
        }
    }
}
