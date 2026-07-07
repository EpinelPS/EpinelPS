using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/default-attack-skill/set")]
public class DefaultAttackSkillSet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeSetStellarBladeDefaultAttackSkill req = await ReadData<ReqArcadeSetStellarBladeDefaultAttackSkill>();
        User user = GetUser();
        ResArcadeSetStellarBladeDefaultAttackSkill response = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        { 
            stellar.CharacterData.DefaultAttackSkillId = req.AttackSkillId;
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}