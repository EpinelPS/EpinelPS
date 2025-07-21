using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/getfield")]
    public class GetField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetLostSectorFieldData req = await ReadData<ReqGetLostSectorFieldData>();
            User user = GetUser();

            LostSectorData f = user.LostSectorData[req.SectorId];
            ResGetLostSectorFieldData response = new()
            {
                Field = new NetFieldObjectData(),
                Json = f.Json
            };

            foreach (KeyValuePair<string, NetLostSectorFieldObject> item in f.Objects)
                response.Field.Objects.Add(new NetFieldObject()
                {
                    ActionAt = item.Value.ActionAt,
                    Json = item.Value.Json,
                    PositionId = item.Key,
                    Type = item.Value.Type
                });


            foreach (KeyValuePair<string, int> item in f.ClearedStages)
                response.Field.Stages.Add(new NetFieldStageData()
                {
                    PositionId = item.Key,
                    StageId = item.Value
                });

            // 10: lost sector team
            if (user.UserTeams.TryGetValue(10, out NetUserTeamData? value))
                response.Team = value;
            await WriteDataAsync(response);
        }
    }
}
