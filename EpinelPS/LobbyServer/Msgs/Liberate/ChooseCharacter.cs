using EpinelPS.Net;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Liberate
{
    [PacketPath("/liberate/choosecharacter")]
    public class ChooseCharacter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ChooseLiberateCharacterRequest>();
            var user = GetUser();

            var response = new ChooseLiberateCharacterResponse();

            // TODO
            response.Data = new NetLiberateData() { CharacterId = req.CharacterId };
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 1 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 2 });
            response.Data.MissionData.Add(new NetLiberateMissionData() { MissionState = LiberateMissionState.Running, Id = 3 });

            await WriteDataAsync(response);
        }
    }
}
