using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Liberate
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
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.LiberateMissionStateRunning, Id = 1 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.LiberateMissionStateRunning, Id = 2 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.LiberateMissionStateRunning, Id = 3 });

            await WriteDataAsync(response);
        }
    }
}
