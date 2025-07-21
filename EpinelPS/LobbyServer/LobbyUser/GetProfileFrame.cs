using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetProfileFrame")]
    public class GetProfileFrame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetProfileFrame req = await ReadData<ReqGetProfileFrame>();
            ResGetProfileFrame response = new();

            foreach (UserFrameTableRecord frameRecord in GameData.Instance.userFrameTable.Values)
            {
                response.Frames.Add(frameRecord.id);
            }

            await WriteDataAsync(response);
        }
    }
}
