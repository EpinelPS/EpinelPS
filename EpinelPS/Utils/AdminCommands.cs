using DnsClient;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Models.Admin;
using System.Net;

namespace EpinelPS.Utils
{
    public class AdminCommands
    {

        public static RunCmdResponse CompleteStage(ulong userId, string input2)
        {
            var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
            if (user == null) return new RunCmdResponse() { error = "invalid user ID" };

            try
            {
                var chapterParsed = int.TryParse(input2.Split('-')[0], out int chapterNumber);
                var stageParsed = int.TryParse(input2.Split('-')[1], out int stageNumber);

                if (chapterParsed && stageParsed)
                {
                    Console.WriteLine($"Chapter number: {chapterNumber}, Stage number: {stageNumber}");

                    // Complete main stages
                    for (int i = 0; i <= chapterNumber; i++)
                    {
                        var stages = GameData.Instance.GetStageIdsForChapter(i, true);
                        int target = 1;
                        foreach (var item in stages)
                        {
                            if (!user.IsStageCompleted(item, true))
                            {
                                Console.WriteLine("Completing stage " + item);
                                ClearStage.CompleteStage(user, item, true);
                            }

                            if (i == chapterNumber && target == stageNumber)
                            {
                                break;
                            }

                            target++;
                        }
                    }

                    // Process scenario and regular stages
                    Console.WriteLine($"Processing stages for chapters 0 to {chapterNumber}");

                    for (int chapter = 0; chapter <= chapterNumber; chapter++)
                    {
                        Console.WriteLine($"Processing chapter: {chapter}");

                        var stages = GameData.Instance.GetScenarioStageIdsForChapter(chapter)
                            .Where(stageId => GameData.Instance.IsValidScenarioStage(stageId, chapterNumber, stageNumber))
                            .ToList();

                        Console.WriteLine($"Found {stages.Count} stages for chapter {chapter}");

                        foreach (var stage in stages)
                        {
                            if (!user.CompletedScenarios.Contains(stage))
                            {
                                user.CompletedScenarios.Add(stage);
                                Console.WriteLine($"Added stage {stage} to CompletedScenarios");
                            }
                            else
                            {
                                Console.WriteLine($"Stage {stage} is already completed");
                            }
                        }
                    }

                    // Save changes to user data
                    JsonDb.Save();
                }
                else
                {
                    return new RunCmdResponse() { error = "Chapter and stage number must be valid integers" };
                }
            }
            catch (Exception ex)
            {
                return new RunCmdResponse() { error = "Exception: " + ex.ToString() };
            }

            return RunCmdResponse.OK;
        }

        public static RunCmdResponse AddAllCharacters(User user)
        {
            // Group characters by name_code and always add those with grade_core_id == 11, 103, and include grade_core_id == 201
            var allCharacters = GameData.Instance.CharacterTable.Values
                .GroupBy(c => c.name_code)  // Group by name_code to treat same name_code as one character                     3999 = marian
                .SelectMany(g => g.Where(c => c.grade_core_id == 1 || c.grade_core_id == 101 || c.grade_core_id == 201 || c.name_code == 3999))  // Always add characters with grade_core_id == 11 and 103
                .ToList();

            foreach (var character in allCharacters)
            {
                if (!user.HasCharacter(character.id))
                {
                    user.Characters.Add(new Database.Character()
                    {
                        CostumeId = 0,
                        Csn = user.GenerateUniqueCharacterId(),
                        Grade = 0,
                        Level = 1,
                        Skill1Lvl = 1,
                        Skill2Lvl = 1,
                        Tid = character.id,  // Tid is the character ID
                        UltimateLevel = 1
                    });

                    user.BondInfo.Add(new() { NameCode = character.name_code, Level = 1 });
                    user.AddTrigger(TriggerType.ObtainCharacter, 1, character.name_code);
                    user.AddTrigger(TriggerType.ObtainCharacterNew, 1);
                }
            }

            JsonDb.Save();

            return RunCmdResponse.OK;
        }

        public static RunCmdResponse AddAllMaterials(User user, int amount)
        {
            foreach (var tableItem in GameData.Instance.itemMaterialTable.Values)
            {
                ItemData? item = user.Items.FirstOrDefault(i => i.ItemType == tableItem.id);

                if (item == null)
                {
                    user.Items.Add(new ItemData
                    {
                        Isn = user.GenerateUniqueItemId(),
                        ItemType = tableItem.id,
                        Level = 1,
                        Exp = 1,
                        Count = amount
                    });
                }
                else
                {
                    item.Count += amount;
                }
            }

            Console.WriteLine($"Added {amount} of all materials to user " + user.Username);
            JsonDb.Save();
            return RunCmdResponse.OK;
        }
    }
}
