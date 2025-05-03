using EpinelPS.Utils;
using EpinelPS.Data;
namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/getwallpaperinventory")]
    public class GetWallpaperInventory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWallpaperInventory>();

            // Prepare the response
            var r = new ResGetWallpaperInventory();

            // Fetch all the wallpaper IDs from the LiveWallpaperTable,
            // excluding records where livewallpaper_type is "SkillCutScene"
            var wallpaperIds = GameData.Instance.lwptablemgrs.Where(w => w.Value.livewallpaper_type != "SkillCutScene").Select(w => w.Key).ToList();

            // Add the filtered wallpaper IDs to the LivewallpaperIds field
            r.LivewallpaperIds.AddRange(wallpaperIds);

            // Send the response back
            await WriteDataAsync(r);
        }
    }
}
