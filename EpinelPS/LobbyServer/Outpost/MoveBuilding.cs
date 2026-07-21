using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/Outpost/MoveBuilding")]
public class MoveBuilding : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMoveBuilding req = await ReadData<ReqMoveBuilding>();
        User user = GetUser();

        NetUserOutpostData? src = user.OutpostBuildings.FirstOrDefault(x => x.SlotId == req.SrcPositionId);
        NetUserOutpostData? dst = user.OutpostBuildings.FirstOrDefault(x => x.SlotId == req.DstPositionId);

        if (src != null && dst != null)
        {
            (src.SlotId, dst.SlotId) = (dst.SlotId, src.SlotId);
            JsonDb.Save();
        }

        ResMoveBuilding response = new();
        await WriteDataAsync(response);
    }
}
