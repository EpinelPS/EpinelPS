﻿using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/recycleroom/get")]
    public class GetRecycleRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetRecycleRoomData>();
            var user = GetUser();
            var response = new ResGetRecycleRoomData();

            response.Recycle.AddRange(user.ResearchProgress.Select(progress =>
            {
                return new NetUserRecycleRoomData()
                {
                    Tid = progress.Key,
                    Lv = progress.Value.Level,
                    Exp = progress.Value.Exp
                };
            }));

            await WriteDataAsync(response);
        }
    }
}
