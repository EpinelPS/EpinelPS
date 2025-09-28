using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/upgrade")]
    public class DoLimitBreak : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the incoming request that contains the current CSN and ISN
            ReqCharacterUpgrade req = await ReadData<ReqCharacterUpgrade>(); // Contains csn and isn (read-only)
            ResCharacterUpgrade response = new();
            User user = GetUser();

            // Get all character data from the game's character table
            List<CharacterRecord> fullchardata = [.. GameData.Instance.CharacterTable.Values];

            CharacterModel targetCharacter = user.GetCharacterBySerialNumber(req.Csn) ?? throw new NullReferenceException();

            // Find the element with the current csn from the request
            CharacterRecord currentCharacter = fullchardata.FirstOrDefault(c => c.Id == targetCharacter.Tid) ?? throw new NullReferenceException();

            if (currentCharacter != null && targetCharacter != null)
            {
                if (currentCharacter.GradeCoreId == 103 || currentCharacter.GradeCoreId == 11 || currentCharacter.GradeCoreId == 201)
                {
                    Console.WriteLine("cannot limit break any further!");
                    await WriteDataAsync(response);
                    return;
                }

                // Find a new CSN based on the `NameCode` of the current character and `GradeCoreId + 1`
                // For some reason, there is a seperate character for each limit/core break value.
                CharacterRecord? newCharacter = fullchardata.FirstOrDefault(c => c.NameCode == currentCharacter.NameCode && c.GradeCoreId == currentCharacter.GradeCoreId + 1);


                if (newCharacter != null)
                {
                    // replace character in DB with new character
                    targetCharacter.Grade++;
                    targetCharacter.Tid = newCharacter.Id;

                    response.Character = new NetUserCharacterDefaultData()
                    {
                        Csn = req.Csn,
                        CostumeId = targetCharacter.CostumeId,
                        Grade = targetCharacter.Grade,
                        Lv = user.GetSynchroLevel(),
                        Skill1Lv = targetCharacter.Skill1Lvl,
                        Skill2Lv = targetCharacter.Skill2Lvl,
                        Tid = targetCharacter.Tid,
                        UltiSkillLv = targetCharacter.UltimateLevel
                    };

                    // remove spare body item
                    ItemData bodyItem = user.Items.FirstOrDefault(i => i.Isn == req.Isn) ?? throw new NullReferenceException();
                    user.RemoveItemBySerialNumber(req.Isn, 1);
                    response.Items.Add(NetUtils.ToNet(bodyItem));

                    JsonDb.Save();
                }
            }

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
