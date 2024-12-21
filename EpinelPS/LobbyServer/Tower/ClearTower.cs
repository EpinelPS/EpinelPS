using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/cleartower")]
    public class ClearTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearTower>();

            var response = new ResClearTower();
            var user = GetUser();

            if (req.BattleResult == 1)
            {
                response = CompleteTower(user, req.TowerId);
            }

            await WriteDataAsync(response);
        }

        public static ResClearTower CompleteTower(Database.User user, int TowerId)
        {
            var response = new ResClearTower();

            // Parse TowerId to get TowerType and FloorNumber
            int TowerType = (TowerId / 10000) - 1; // For some weird reason the Type here doesn't match up with NetTowerData, thus the -1
            int FloorNumber = TowerId % 10000;

            // Update user's TowerProgress
            if (!user.TowerProgress.ContainsKey(TowerType))
            {
                user.TowerProgress[TowerType] = FloorNumber;
            }
            else if (user.TowerProgress[TowerType] < FloorNumber)
            {
                user.TowerProgress[TowerType] = FloorNumber;
            }

            JsonDb.Save();

            return response;
        }
    }
}