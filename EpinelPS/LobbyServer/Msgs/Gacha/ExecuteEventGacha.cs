//yes i am lazy and its preety much same as exec gacha 
//but only does 1x pull
//its here only so there is no system error on 1x free gacha event
using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/event/execute")]
    public class ExecuteEventGacha : LobbyMsgHandler
    {
        private static readonly Random random = new Random();

        // Exclusion lists for sick pulls mode and normal mode 2500601 is the broken R rarity dorothy
        private static readonly List<int> sickPullsExclusionList = new List<int> { 2500601 }; // Add more IDs as needed
        private static readonly List<int> normalPullsExclusionList = new List<int> { 2500601,422401,306201,399901,399902,399903,399904,201401,301501,112101,313201,319301,319401,320301,422601,426101,328301,328401,235101,235301,136101,339201,140001,140101,140201,580001,580101,580201,581001,581101,581201,582001,582101,582201,583001,583101,583201,583301,190101,290701 }; // Add more IDs as needed

        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExecuteDailyFreeGacha>();
            
            // Count determines whether we select 1 or 10 characters
            int numberOfPulls =  1; 

            var user = GetUser();
            var response = new ResExecuteDailyFreeGacha();

			var entireallCharacterData = GameData.Instance.characterTable.Values.ToList();
			// Remove the .Values part since it's already a list.
			// Group by name_code to treat same name_code as one character 
			// Always add characters with grade_core_id == 1 and 101
			var allCharacterData = entireallCharacterData.GroupBy(c => c.name_code).SelectMany(g => g.Where(c => c.grade_core_id == 1 || c.grade_core_id == 101 || c.grade_core_id == 201 || c.name_code == 3999)).ToList(); 

            // Separate characters by rarity categories
            var rCharacters = allCharacterData.Where(c => c.original_rare == "R" ).ToList();
            var srCharacters = allCharacterData.Where(c => c.original_rare == "SR").ToList();
            
            // Separate Pilgrim SSRs and non-Pilgrim SSRs
            var pilgrimCharacters = allCharacterData.Where(c => c.original_rare == "SSR" && c.corporation == "PILGRIM").ToList();
            var ssrCharacters = allCharacterData.Where(c => c.original_rare == "SSR" && c.corporation != "PILGRIM").ToList();

            var selectedCharacters = new List<CharacterRecord>();

            // Check if user has 'sickpulls' set to true to use old method
            if (user.sickpulls)
            {
                // Old selection method: Randomly select characters based on req.Count value, excluding characters in the sickPullsExclusionList
                selectedCharacters = allCharacterData.Where(c => !sickPullsExclusionList.Contains(c.id)).OrderBy(x => random.Next()).Take(numberOfPulls).ToList(); // Exclude characters based on the exclusion list for sick pulls
            }
            else
            {
                // New method: Select characters based on req.Count value, with each character having its category determined independently, excluding characters in the normalPullsExclusionList
                for (int i = 0; i < numberOfPulls; i++)
                {
                    var character = SelectRandomCharacter(rCharacters, srCharacters, ssrCharacters, pilgrimCharacters, normalPullsExclusionList);
                    selectedCharacters.Add(character);
                }
            }

            var pieceIds = new List<Tuple<int, int>>(); // 2D array to store characterId and pieceId as Tuple
            // Add each character's item to user.Items if the character exists in user.Characters
			foreach (var characterData in selectedCharacters)
			{
				// Check if the item for this character already exists in user.Items based on ItemType
				var existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.piece_id);

				if (existingItem != null)
				{
					// If the item exists, increment the count
					existingItem.Count += 1;

					// Send the updated item in the response
					response.Items.Add(new NetUserItemData()
					{
						Tid = existingItem.ItemType,
						Csn = existingItem.Csn,
						Count = existingItem.Count,
						Level = existingItem.Level,
						Exp = existingItem.Exp,
						Position = existingItem.Position,
						Isn = existingItem.Isn
					});
				}
				else
				{
					// If the item does not exist, create a new item entry
					var newItem = new ItemData()
					{
						ItemType = characterData.piece_id,
						Csn = 0,
						Count = 1, // or any relevant count
						Level = 0,
						Exp = 0,
						Position = 0,
						Corp = 0,
						Isn = user.GenerateUniqueItemId()
					};
					user.Items.Add(newItem);

					// Add the new item to response
					response.Items.Add(new NetUserItemData()
					{
						Tid = newItem.ItemType,
						Csn = newItem.Csn,
						Count = newItem.Count,
						Level = newItem.Level,
						Exp = newItem.Exp,
						Position = newItem.Position,
						Isn = newItem.Isn
					});
				}
			}

            // Populate the 2D array with characterId and pieceId for each selected character
            foreach (var characterData in selectedCharacters)
            {
                var characterId = characterData.id;
                var pieceId = characterData.piece_id;

                // Store characterId and pieceId in the array
                pieceIds.Add(Tuple.Create(characterId, pieceId));
				var id = user.GenerateUniqueCharacterId();
                response.Gacha.Add(new NetGachaEntityData()
                {
                    Corporation = 1,
                    PieceCount = 1,
                    CurrencyValue = 5,
                    Sn = id,
                    Tid = characterId,
                    Type = 1
                });

                // Check if the user already has the character, if not add it
                if (!user.HasCharacter(characterId))
                {
                    response.Characters.Add(new NetUserCharacterDefaultData()
                    {
                        CostumeId = 0,
                        Csn = id,
                        Grade = 0,
                        Level = 1,
                        Skill1Lv = 1,
                        Skill2Lv = 1,
                        Tid = characterId,
                        UltiSkillLv = 1
                    });

                    user.Characters.Add(new Database.Character()
                    {
                        CostumeId = 0,
                        Csn = id,
                        Grade = 0,
                        Level = 1,
                        Skill1Lvl = 1,
                        Skill2Lvl = 1,
                        Tid = characterId,
                        UltimateLevel = 1
                    });
                }
            }



            user.GachaTutorialPlayCount++;
            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private CharacterRecord SelectRandomCharacter(List<CharacterRecord> rCharacters, List<CharacterRecord> srCharacters, List<CharacterRecord> ssrCharacters, List<CharacterRecord> pilgrimCharacters, List<int> exclusionList)
        {
            // Remove excluded characters from each category
            var availableRCharacters = rCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availableSRCharacters = srCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availableSSRCharacters = ssrCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availablePilgrimCharacters = pilgrimCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();

            // Each time we call this method, a new category will be selected for a single character
            double roll = random.NextDouble() * 100; // Roll from 0 to 100

            if (roll < 53 && availableRCharacters.Any())
            {
                // R category
                return availableRCharacters[random.Next(availableRCharacters.Count)];
            }
            else if (roll < 53 + 43 && availableSRCharacters.Any())
            {
                // SR category
                return availableSRCharacters[random.Next(availableSRCharacters.Count)];
            }
            else
            {
                // SSR category
                double ssrRoll = random.NextDouble() * 100;

                if (ssrRoll < 4.55 && availablePilgrimCharacters.Any())
                {
                    // PILGRIM SSR
                    return availablePilgrimCharacters[random.Next(availablePilgrimCharacters.Count)];
                }
                else if (availableSSRCharacters.Any())
                {
                    // Non-PILGRIM SSR
                    return availableSSRCharacters[random.Next(availableSSRCharacters.Count)];
                }
            }

            // Fallback to a random R character if somehow no SSR characters are left after exclusion
            return availableRCharacters.Any() ? availableRCharacters[random.Next(availableRCharacters.Count)] : null;
        }
    }
}