using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/upgrade")]
    public class DoLimitBreak : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the incoming request that contains the current CSN and ISN
            var req = await ReadData<ReqCharacterUpgrade>(); // Contains csn and isn (read-only)
            var response = new ResCharacterUpgrade();
            var user = GetUser();

            // Get all character data from the game's character table
            var fullchardata = GameData.Instance.characterTable.Values.ToList();

            var targetCharacter = user.GetCharacterBySerialNumber(req.Csn);

            // Find the element with the current csn from the request
            var currentCharacter = fullchardata.FirstOrDefault(c => c.id == targetCharacter.Tid);

            if (currentCharacter != null && targetCharacter != null)
            {
                if (currentCharacter.grade_core_id == 103 || currentCharacter.grade_core_id == 11 || currentCharacter.grade_core_id == 201)
                {
                    Console.WriteLine("cannot limit break any further!");
                    await WriteDataAsync(response);
                    return;
                }

                // Find a new CSN based on the `name_code` of the current character and `grade_core_id + 1`
                // For some reason, there is a seperate character for each limit/core break value.
                var newCharacter = fullchardata.FirstOrDefault(c => c.name_code == currentCharacter.name_code && c.grade_core_id == currentCharacter.grade_core_id + 1);


                if (newCharacter != null)
                {
                    // replace character in DB with new character
                    targetCharacter.Grade++;
                    targetCharacter.Tid = newCharacter.id;

                    response.Character = new NetUserCharacterDefaultData()
                    {
                        Csn = req.Csn,
                        CostumeId = targetCharacter.CostumeId,
                        Grade = targetCharacter.Grade,
                        Level = user.GetSynchroLevel(),
                        Skill1Lv = targetCharacter.Skill1Lvl,
                        Skill2Lv = targetCharacter.Skill2Lvl,
                        Tid = targetCharacter.Tid,
                        UltiSkillLv = targetCharacter.UltimateLevel
                    };

                    // remove spare body item
                    var bodyItem = user.Items.FirstOrDefault(i => i.Isn == req.Isn);
                    user.RemoveItemBySerialNumber(req.Isn, 1);
                    response.Items.Add(NetUtils.ToNet(bodyItem));

                    // replace any reference to the old character to the new TID
                    // Check if RepresentationTeamData exists and has slots
                    if (user.RepresentationTeamData != null && user.RepresentationTeamData.Slots != null)
                    {
                        // Iterate through RepresentationTeamData slots
                        foreach (var slot in user.RepresentationTeamData.Slots)
                        {
                            // Find the character in user's character list that matches the slot's Tid
                            var correspondingCharacter = user.Characters.FirstOrDefault(c => c.Tid == slot.Tid);

                            if (correspondingCharacter != null)
                            {
                                // Update the CSN value if it differs
                                if (slot.Csn != correspondingCharacter.Csn)
                                {
                                    slot.Csn = correspondingCharacter.Csn;
                                }
                            }
                        }
                    }

                    JsonDb.Save();
                }
            }

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
