﻿using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqStoryDungeonEventData>();
            var user = GetUser();

            var response = new ResStoryDungeonEventData()
            {
                TeamData = new NetUserTeamData()
            };

            // TOOD

            await WriteDataAsync(response);
        }
    }
}