using EpinelPS.Utils;


namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/jukebox/playlist/get")]
    public class JukeboxPlaylistGet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Prepare response with static data
            ResGetJukeboxPlaylist response = new()
            {
                Playlists = {}, // Assuming Playlists is a list or similar collection type, you may want to add items here.
                FavoriteSongs = new NetJukeboxFavorite
                {
                    Songs = 
                    {
                        new NetJukeboxPlaylistSong { JukeboxTableId = 8995001,Order = 899 }, 
                        new NetJukeboxPlaylistSong { JukeboxTableId = 9020001, Order = 902 }  
                    }
                },
                // JukeboxPlaylistUId = 123456789 // Assign a static UID, if required
            };

            // Send the response
            await WriteDataAsync(response);
        }
    }
}
