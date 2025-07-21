using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/Get")]
    public class GetUser : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetUserData req = await ReadData<ReqGetUserData>();
            ResGetUserData response = new();
            User user = GetUser();

            TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
            long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);


            response.User = LobbyHandler.CreateNetUserDataFromUser(user);
            response.ResetHour = JsonDb.Instance.ResetHourUtcTime;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.IsSimple = req.IsSimple;

            foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }
            response.RepresentationTeam = NetUtils.GetDisplayedTeam(user);

            response.LastClearedNormalMainStageId = user.LastNormalStageCleared;

            // Restore completed tutorials. GroupID is the first 4 digits of the Table ID.
            foreach (KeyValuePair<int, Data.ClearedTutorialData> item in user.ClearedTutorialData)
            {
                int groupId = item.Value.GroupId;
                int version = item.Value.VersionGroup;

                response.User.Tutorials.Add(new NetTutorialData() { GroupId = groupId, LastClearedTid = item.Key, LastClearedVersion = version });
            }

            response.CommanderRoomJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = user.CommanderMusic.TableId, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.CommanderRoom };
            response.LobbyJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = user.LobbyMusic.TableId, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.Lobby };

            await WriteDataAsync(response);
        }
    }
}
