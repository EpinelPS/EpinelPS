using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/getoutpostdata")]
    public class GetOutpostData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetOutpostData>();
            var user = GetUser();

            var battleTime = DateTime.UtcNow - user.BattleTime;
            var battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);


            var response = new ResGetOutpostData
            {
                OutpostBattleLevel = new NetOutpostBattleLevel() { Level = 1 },
                CommanderBgm = new NetUserJukeboxDataV2() { CommandBgm = new() { Type = NetJukeboxBgmType.JukeboxTableId, JukeboxTableId = 5 } },
                BattleTime = 864000000000,
                Jukebox = new(),
                MaxBattleTime = 864000000000,
                SkinGroupId = 1000
            };

            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
            response.Data.Add(new NetUserOutpostData() { SlotId = 1, BuildingId = 22401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 4, BuildingId = 22701, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 5, BuildingId = 22801, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 6, BuildingId = 22901, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 7, BuildingId = 23001, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 3, BuildingId = 23101, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 2, BuildingId = 23201, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 9, BuildingId = 23301, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 8, BuildingId = 23401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 10, BuildingId = 23501, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Data.Add(new NetUserOutpostData() { SlotId = 38, BuildingId = 33601, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });

            // TODO
            await WriteDataAsync(response);
        }
    }
}
