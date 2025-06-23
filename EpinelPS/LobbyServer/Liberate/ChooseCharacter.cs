using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate
{
    [PacketPath("/liberate/choosecharacter")]
    public class ChooseCharacter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqChooseLiberateCharacter>();
            var user = GetUser();

            var response = new ResChooseLiberateCharacter();

            // TODO
            response.Data = new NetLiberateData() { CharacterId = req.CharacterId };
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 1 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 2 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 3 });

            await WriteDataAsync(response);
        }
    }
}
