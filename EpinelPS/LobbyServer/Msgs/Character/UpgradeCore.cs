using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.StaticInfo;
//this file is work in progress and currently its fucked 
namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/coreupgrade")]
    public class CoreUpgrade : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the incoming request that contains the current CSN and ISN
            var req = await ReadData<ReqCharacterCoreUpgrade>(); // Contains csn and isn (read-only)
            var response = new ResCharacterCoreUpgrade();

            // Get all character data from the game's character table
            var fullchardata = GameData.Instance.characterTable.Values.ToList();
            
            // Find the element with the current csn from the request
            var currentCharacter = fullchardata.FirstOrDefault(c => c.id == req.Csn);
            
            if (currentCharacter != null)
            {
                // Find a new CSN based on the `name_code` of the current character and `grade_core_id + 1`
                var newCharacter = fullchardata.FirstOrDefault(c => c.name_code == currentCharacter.name_code && c.grade_core_id == currentCharacter.grade_core_id + 1);
                
                if (newCharacter != null)
                {
                    // Update the characterData with the new CSN
                    var characterData = new NetUserCharacterDefaultData
                    {
                        Csn = newCharacter.id
                        // Add any other required data here
                    };

                }

            }
            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}