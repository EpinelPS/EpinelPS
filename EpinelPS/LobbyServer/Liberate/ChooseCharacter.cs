namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/choosecharacter")]
public class ChooseCharacter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqChooseLiberateCharacter req = await ReadData<ReqChooseLiberateCharacter>();
        User user = GetUser();

        ResChooseLiberateCharacter response = new()
        {
            // TODO
            Data = new NetLiberateData() { CharacterId = req.CharacterId }
        };
        response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 1 });
        response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 2 });
        response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 3 });

        await WriteDataAsync(response);
    }
}
