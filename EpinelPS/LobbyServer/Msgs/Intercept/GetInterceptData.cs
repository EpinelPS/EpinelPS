using EpinelPS.Net;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/get")]
    public class GetInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetInterceptData>();

            var response = new ResGetInterceptData
            {
                NormalInterceptGroup = 1,
                SpecialInterceptId = 1,
                TicketCount = 5,
                MaxTicketCount = 10
            };

            await WriteDataAsync(response);
        }
    }
}
