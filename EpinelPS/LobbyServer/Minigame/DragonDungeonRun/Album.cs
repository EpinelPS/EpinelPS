using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/album")]
public class Album : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDragonDungeonRunAlbum req = await ReadData<ReqGetArcadeDragonDungeonRunAlbum>();
        User user = GetUser();
        ResGetArcadeDragonDungeonRunAlbum response = new();

        if (user.DDRDatas.CutSceneList.Count > 0)
        {
            foreach (var item in user.DDRDatas.CutSceneList)
            {
                response.CutSceneList.Add(new ResGetArcadeDragonDungeonRunAlbum.Types.NetArcadeDragonDungeonRunCutSceneData()
                {
                    CutSceneId = item.Key,
                    IsNew = item.Value
                });
                user.DDRDatas.CutSceneList[item.Key] = false;
            }

            user.DDRDatas.HasUnconfirmedAlbum = false;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}