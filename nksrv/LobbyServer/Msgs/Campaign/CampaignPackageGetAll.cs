using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Campaign
{
    [PacketPath("/shutdownflags/campaignpackage/getall")]
    public class CampaignPackageGetAll : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCampaignPackageGetAllShutdownFlags>();

            var response = new ResCampaignPackageGetAllShutdownFlags();
            // TODO
            WriteData(response);
        }
    }
}
