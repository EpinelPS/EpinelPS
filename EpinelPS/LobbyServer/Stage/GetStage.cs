using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stage/get")]
public class GetStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetStageData req = await ReadData<ReqGetStageData>();
        User user = GetUser();

        string mapId = GameData.Instance.GetMapIdFromChapter(req.Chapter, (ChapterMod)req.Mod);

        ResGetStageData response = new()
        {
            Field = CreateFieldInfo(user, mapId, out bool bossEntered),
            HasChapterBossEntered = bossEntered,
            SquadData = ""
        };

        await WriteDataAsync(response);
    }

    public static NetFieldObjectData CreateFieldInfo(User user, string mapId, out bool BossEntered)
    {
        NetFieldObjectData f = new();
        bool found = false;
        BossEntered = false;

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == mapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = mapId
            };
            user.FieldInfo.Add(field);
        }

        foreach (int stage in field.CompletedStages)
        {
            f.Stages.Add(new NetFieldStageData() { StageId = stage });
        }
        foreach (var obj in field.CompletedObjects)
        {
            f.Objects.Add(new NetFieldObject()
            {
                PositionId = obj.PositionId,
                ActionAt = obj.ActionAt.Ticks,
                Json = obj.Json,
                Type = obj.Type
            });
        }
        BossEntered = field.BossEntered;

        return f;
    }
}
