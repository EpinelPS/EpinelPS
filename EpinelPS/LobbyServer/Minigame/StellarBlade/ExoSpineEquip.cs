using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/exo-spine/equip")]
public class ExoSpineEquip : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeEquipStellarBladeExoSpine req = await ReadData<ReqArcadeEquipStellarBladeExoSpine>();
        User user = GetUser();
        ResArcadeEquipStellarBladeExoSpine response = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            stellar.CharacterData.ExoSpineSbItemId = req.ExoSpineItemId;
        }        

        JsonDb.Save();     
        // TODO
        await WriteDataAsync(response);
    }
}