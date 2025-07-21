using EpinelPS.Utils;
using EpinelPS.Data;
namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/getoutpostdata")]
    public class GetOutpostData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetOutpostData req = await ReadData<ReqGetOutpostData>();
            User user = GetUser();
            user.ResetDataIfNeeded();

            TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
            long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);

            long overBattleTime = battleTimeMs > 12096000000000 ? battleTimeMs - 12096000000000 : 0;


            ResGetOutpostData response = new()
            {
                OutpostBattleLevel = user.OutpostBattleLevel,
                Jukeboxv2 = new NetUserJukeboxDataV2() { CommandBgm = new() { Type = NetJukeboxBgmType.JukeboxTableId, JukeboxTableId = user.CommanderMusic.TableId } }
            };

            // TODO: do not hard code this!
            //response.Jukebox.List.AddRange([5, 9999901, 4001, 4002, 4003, 4004, 4005, 4006, 12, 4007, 4008, 4009, 4010, 4011, 4012, 4013, 4014, 4015, 4016, 4017, 4018, 4019, 4020, 4021, 4022, 4023, 4024, 4025, 4026, 4027, 4028, 4029, 4030, 4031, 4032, 4033, 4034, 4036, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010, 5011, 5012]);
            //response.JukeboxV2.JukeboxTableIds.AddRange([5, 9999901, 4001, 4002, 4003, 4004, 4005, 4006, 12, 4007, 4008, 4009, 4010, 4011, 4012, 4013, 4014, 4015, 4016, 4017, 4018, 4019, 4020, 4021, 4022, 4023, 4024, 4025, 4026, 4027, 4028, 4029, 4030, 4031, 4032, 4033, 4034, 4036, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010, 5011, 5012]);


            // Directly use jukeboxListDataRecords
            List<int> jukeboxIds = [.. GameData.Instance.jukeboxListDataRecords.Keys];

            // Update response lists with the IDs
            response.Jukeboxv2.JukeboxTableIds.AddRange(jukeboxIds);
			
            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs, OverBattleTime = overBattleTime };
            
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

            response.TimeRewardBuffs.AddRange(NetUtils.GetOutpostTimeReward(user));

            // TODO
            await WriteDataAsync(response);
        }
    }
}
