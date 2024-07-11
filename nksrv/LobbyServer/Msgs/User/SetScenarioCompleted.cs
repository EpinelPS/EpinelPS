using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/SetScenarioComplete")]
    public class SetScenarioCompleted : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetScenarioComplete>();

            var response = new ResSetScenarioComplete();

            // Mark the story "scenario" as completed.
            // TODO: Get rewards by making a database of them from actual server.
            response.Reward = new NetRewardData();


            var user = JsonDb.GetUser(UserId);
            if (user == null)
            {
                throw new Exception("null user in SetScenarioComplete command");
            }
            user.CompletedScenarios.Add(req.ScenarioId);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
