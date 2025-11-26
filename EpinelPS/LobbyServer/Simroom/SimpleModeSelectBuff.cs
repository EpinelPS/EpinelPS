using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/simplemode/selectbuff")]
    public class SimpleModeSelectBuff : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "buffToAdd": 1010106, "buffToDelete": 1010105 }
            ReqSelectSimRoomSimpleModeBuff req = await ReadData<ReqSelectSimRoomSimpleModeBuff>();
            User user = GetUser();


            List<int> buffs = user.ResetableData.SimRoomData.Buffs;
            List<int> legacyBuffs = user.ResetableData.SimRoomData.LegacyBuffs;
            if (req.BuffToDelete > 0)
            {
                if (buffs.Contains(req.BuffToDelete)) buffs.Remove(req.BuffToDelete);
                if (legacyBuffs.Contains(req.BuffToDelete)) legacyBuffs.Remove(req.BuffToDelete);
            }

            if (req.BuffToAdd > 0)
            {
                if (!buffs.Contains(req.BuffToDelete)) buffs.Add(req.BuffToDelete);
                if (!legacyBuffs.Contains(req.BuffToDelete)) legacyBuffs.Add(req.BuffToDelete);
            }

            ResSelectSimRoomSimpleModeBuff response = new()
            {
                Result = SimRoomResult.Reset,
                NextSimpleModeBuffSelectionInfo = new()
                {
                    RemainingBuffSelectCount = 8 - buffs.Count,
                    BuffOptions = { buffs },
                },
            };

            user.ResetableData.SimRoomData.Entered = false;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}