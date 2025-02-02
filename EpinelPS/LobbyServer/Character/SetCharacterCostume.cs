﻿using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/costume/set")]
    public class SetCharacterCostume : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetCharacterCostume>();
            var user = GetUser();

            foreach (var item in user.Characters)
            {
                if (item.Csn == req.Csn)
                {
                    item.CostumeId = req.CostumeId;
                    break;
                }
            }
            JsonDb.Save();

            var response = new ResSetCharacterCostume();

            await WriteDataAsync(response);
        }
    }
}
