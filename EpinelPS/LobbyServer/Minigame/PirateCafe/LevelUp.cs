using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.PirateCafe;

[GameRequest("/arcade/PirateCafe/levelup")]
public class LevelUp : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLevelUpArcadePirateCafeSkill req = await ReadData<ReqLevelUpArcadePirateCafeSkill>();
        User user = GetUser();
        ResLevelUpArcadePirateCafeSkill response = new();

        user.PirateCafeData.SkillLevel = req.SkillLevel;
        response.Data = MiniGameHelper.ToProto<NetArcadePirateCafeData, ArcadePirateCafeData>(user.PirateCafeData);
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}