using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/gear/equip")]
public class GearEquip : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeEquipStellarBladeGear req = await ReadData<ReqArcadeEquipStellarBladeGear>();
        User user = GetUser();
        ResArcadeEquipStellarBladeGear response = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            switch (req.Slot)
            {
                case GearSlot.Unspecified:
                    break;
                case GearSlot.Slot1:
                    stellar.CharacterData.Gear1SbItemId = req.SbItemId;
                    break;
                case GearSlot.Slot2:
                    stellar.CharacterData.Gear2SbItemId = req.SbItemId;
                    break;
                default:
                    break;
            }

        }       

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}