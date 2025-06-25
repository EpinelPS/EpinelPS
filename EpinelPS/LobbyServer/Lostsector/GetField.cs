using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/getfield")]
    public class GetField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLostSectorFieldData>();
            var user = GetUser();

            var f = user.LostSectorData[req.SectorId];
            ResGetLostSectorFieldData response = new()
            {
                Field = new NetFieldObjectData(),
                Json = f.Json
            };

            foreach (var item in f.Objects)
                response.Field.Objects.Add(new NetFieldObject()
                {
                    ActionAt = item.Value.ActionAt,
                    Json = item.Value.Json,
                    PositionId = item.Key,
                    Type = item.Value.Type
                });


            foreach (var item in f.ClearedStages)
                response.Field.Stages.Add(new NetFieldStageData()
                {
                    PositionId = item.Key,
                    StageId = item.Value
                });

            // 10: lost sector team
            if (user.UserTeams.ContainsKey(10))
                response.Team = user.UserTeams[10];
            await WriteDataAsync(response);
        }
    }
}
