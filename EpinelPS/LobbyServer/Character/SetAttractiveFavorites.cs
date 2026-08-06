using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/attractive/setfavorites")]
public class SetAttractiveFavorites : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetAttractiveFavorites req = await ReadData<ReqSetAttractiveFavorites>();
        User user = GetUser();

        NetUserAttractiveData? bond = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);
        if (bond != null)
        {
            bond.IsFavorites = req.IsFavorites;
            JsonDb.Save();
        }

        await WriteDataAsync(new ResSetAttractiveFavorites());
    }
}
