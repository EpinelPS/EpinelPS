using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/skill/levelupV2")]
public class SkillLevelUpV2 : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCharacterSkillLevelUpV2 req = await ReadData<ReqCharacterSkillLevelUpV2>();
        User user = GetUser();
        ResCharacterSkillLevelUpV2 response = new();

        SkillLevelUpResult result = SkillLevelUpHelper.Upgrade(user, req.Csn, req.SkillCategory, req.TargetLevel);
        response.Character = result.Character;
        response.Items.AddRange(result.Items);
        response.Currencies.AddRange(result.Currencies);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
