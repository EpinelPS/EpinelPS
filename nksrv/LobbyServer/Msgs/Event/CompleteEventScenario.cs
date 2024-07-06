using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/event/scenario/complete")]
    public class CompleteEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetEventScenarioComplete>();
            var user = GetUser();

            if (user.EventInfo.ContainsKey(req.EventID))
            {
                var evt = user.EventInfo[req.EventID];
                evt.CompletedScenarios.Add(req.ScenarioId);
            }
            else
            {
                var evt = new EventData();
                evt.CompletedScenarios.Add(req.ScenarioId);
                user.EventInfo.Add(req.EventID, evt);
            }

            var response = new ResSetEventScenarioComplete();

            // TODO reward

            WriteData(response);
        }
    }
}
