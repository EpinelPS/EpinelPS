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
                var newCharacter = fullchardata.FirstOrDefault(c => c.name_code == currentCharacter.name_code && c.grade_core_id == currentCharacter.grade_core_id + 1);


                if (newCharacter != null)
                {
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

                    // TODO: remove spare body
                    foreach (var item in user.Items)
                    {
                        response.Items.Add(NetUtils.ToNet(item));
                    }

                    JsonDb.Save();
                }

            }

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
