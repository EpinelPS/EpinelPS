﻿using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/gettowerdata")]
    public class GetTowerData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetTowerData req = await ReadData<ReqGetTowerData>();

            ResGetTowerData response = new();

            User user = GetUser();

            // TODO: Load remain count for these
            NetTowerData t0 = new() { Type = 1, RemainCount = 3 };
            NetTowerData t1 = new() { Type = 2, RemainCount = 3 };
            NetTowerData t2 = new() { Type = 3, RemainCount = 3 };
            NetTowerData t3 = new() { Type = 4, RemainCount = 3 };
            NetTowerData t4 = new() { Type = 5 };

            // setup schedules
            t0.Schedules.Add(new NetSchedule() { DayOfWeek = new() { DayOfWeeks = { 1, 4, 6 }, StartTime = 720000000000, Duration = 863990000000 } });
            t1.Schedules.Add(new NetSchedule() { DayOfWeek = new() { DayOfWeeks = { 2, 5, 6 }, StartTime = 720000000000, Duration = 863990000000 } });
            t2.Schedules.Add(new NetSchedule() { DayOfWeek = new() { DayOfWeeks = { 0, 3, 6 }, StartTime = 720000000000, Duration = 863990000000 } });
            t3.Schedules.Add(new NetSchedule() { DayOfWeek = new() { DayOfWeeks = { 2, 6 }, StartTime = 720000000000, Duration = 863990000000 } });
            t4.Schedules.Add(new NetSchedule() { AllTime = new() });

            if (user.TowerProgress.TryGetValue(1, out int floor1))
                t0.Floor = floor1;
            if (user.TowerProgress.TryGetValue(2, out int floor2))
                t1.Floor = floor2;
            if (user.TowerProgress.TryGetValue(3, out int floor3))
                t2.Floor = floor3;
            if (user.TowerProgress.TryGetValue(4, out int floor4))
                t3.Floor = floor4;
            if (user.TowerProgress.TryGetValue(5, out int floor5))
                t4.Floor = floor5;

            response.Data.Add(t0);
            response.Data.Add(t1);
            response.Data.Add(t2);
            response.Data.Add(t3);
            response.Data.Add(t4);

            await WriteDataAsync(response);
        }
    }
}
