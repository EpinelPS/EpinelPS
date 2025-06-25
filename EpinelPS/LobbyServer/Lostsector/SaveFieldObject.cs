using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/savefieldobject")]
    public class SaveFieldObject : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSaveLostSectorFieldObject>();
            var user = GetUser();

            var response = new ResSaveLostSectorFieldObject();

            if (user.LostSectorData[req.SectorId].Objects.ContainsKey(req.Object.PositionId))
                user.LostSectorData[req.SectorId].Objects[req.Object.PositionId] = req.Object;
            else
                user.LostSectorData[req.SectorId].Objects.Add(req.Object.PositionId, req.Object);

            if(req.Object.TeamPosition.Count != 0)
            {
                user.LostSectorData[req.SectorId].TeamPositions.Clear();
                user.LostSectorData[req.SectorId].TeamPositions.AddRange(req.Object.TeamPosition);
            }

            await WriteDataAsync(response);
        }
    }
}
