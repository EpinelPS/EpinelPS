using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/upgradeequipment")]
    public class UpgradeDaveEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUpgradeDaveEquipment>();

            var response = new ResUpgradeDaveEquipment
            {

            };

            await WriteDataAsync(response);
        }
    }
}
