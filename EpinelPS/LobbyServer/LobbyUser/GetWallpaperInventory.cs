using EpinelPS.Utils;
using EpinelPS.Data;
namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/getwallpaperinventory")]
    public class GetWallpaperInventory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetWallpaperInventory req = await ReadData<ReqGetWallpaperInventory>();

            // Prepare the response
            ResGetWallpaperInventory r = new();

            // Fetch all the wallpaper IDs from the LiveWallpaperTable,
            // excluding records where livewallpaper_type is "SkillCutScene"
            List<int> wallpaperIds = [.. GameData.Instance.lwptablemgrs.Where(w => w.Value.LivewallpaperType != Livewallpaper_type.SkillCutScene).Select(w => w.Key)];

            // Add the filtered wallpaper IDs to the LivewallpaperIds field
            r.LivewallpaperIds.AddRange(wallpaperIds);

            // Send the response back
            await WriteDataAsync(r);
        }
    }
}
