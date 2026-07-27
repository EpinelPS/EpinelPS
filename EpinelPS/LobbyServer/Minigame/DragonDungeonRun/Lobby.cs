using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/lobby")]
public class Lobby : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeDragonDungeonRunLobby req = await ReadData<ReqEnterArcadeDragonDungeonRunLobby>();
        User user = GetUser();
        ResEnterArcadeDragonDungeonRunLobby response = new()
        {
            UserRankingData = new()
            { User = LobbyHandler.CreateWholeUserDataFromDbUser(user) }
        };

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.DragonDungeonRun).FirstOrDefault();

        if (user.Guild.guildId > 0)
        {
            var rankinfo = MiniGameHelper.GetGuildUserRank((long)user.Guild.guildId, user.ID, arcade.Id);
            if (rankinfo != null)
            {
                var guild = new NetMiniGameDragonDungeonRunRankingData()
                {
                    Rank = rankinfo.Value.Rank,
                    Score = (int)rankinfo.Value.Record.Score,
                    User = LobbyHandler.CreateWholeUserDataFromDbUser(user)
                };

                response.UserRankingData = guild;
            }
        }

        InitDdrData(user, MiniGameSystemType.Arcade);

        response.HasUnconfirmedAlbum = user.DDRDatas.HasUnconfirmedAlbum;
        response.HasWatchedTutorial = user.DDRDatas.HasWatchedTutorial;
        response.NewScenarioAvailable = user.DDRDatas.NewScenarioAvailable;

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }

    public void InitDdrData(User user, MiniGameSystemType arcade)
    {
        EventDragonDungeonRunManagerRecord_Raw? ddrmanager = GameData.Instance.EventDragonDungeonRunManagerTable.Values
                .Where(x => x.MinigameType == arcade).FirstOrDefault();

        if (user.DDRDatas.Characters.Count == 0)
        {
            List<EventDragonDungeonRunCharacterRecord_Raw>? chars = GameData.Instance.EventDragonDungeonRunCharacterTable.Values
                .Where(x => x.GroupId == ddrmanager.UseCharacterGroupId && x.OpenCondition == EventDragonDungeonRunCharacterUnlockType.None).ToList();
            List<int> firstchar = chars.Select(x => x.Id).ToList();
            user.DDRDatas.Characters.AddRangeUnique(firstchar);
            user.DDRDatas.NewCharacters.AddRangeUnique(firstchar);
            user.DDRDatas.LastPlayCharacter = firstchar.FirstOrDefault();
        }

        if (user.DDRDatas.MissionDatas.Count == 0)
        {
            List<EventDragonDungeonRunMissionRecord_Raw>? missionlist = GameData.Instance.EventDragonDungeonRunMissionTable.Values
                .Where(x => x.GroupId == ddrmanager.MissionGroupId).ToList();
            foreach (var item in missionlist)
            {
                user.DDRDatas.MissionDatas.TryAdd(item.Id, new()
                {
                    MissionTargetId = item.MissionTargetId,
                    MissionType = (int)item.MissionType,
                    Progress = 0
                });
            }
        }
    }
}