using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/ProfileCard/DecorationLayout/Get")]
    public class GetProfileDecoration : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqProfileCardDecorationLayout>();

            var r = new ResProfileCardDecorationLayout();
            r.Layout = new ProfileCardDecorationLayout();
            r.Layout.BackgroundId = 101002;
            await WriteDataAsync(r);
        }
    }
}
