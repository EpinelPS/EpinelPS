using EpinelPS.Utils;
using EpinelPS.StaticInfo;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetProfileFrame")]
    public class GetProfileFrame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetProfileFrame>();
            var response = new ResGetProfileFrame();

            foreach (var frameRecord in GameData.Instance.userFrameTable.Values)
            {
                response.Frames.Add(frameRecord.id);
            }

            await WriteDataAsync(response);
        }
    }
}
