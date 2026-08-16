//yes i am lazy and its preety much same as exec gacha 
//but only does 1x pull
//its here only so there is no system error on 1x free gacha event

using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/event/execute")]
public class ExecuteEventGacha : LobbyMessage
{
    private static readonly Random random = new();

    // Exclusion lists for sick pulls mode and normal mode 2500601 is the broken R rarity dorothy
    private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
    private static readonly List<int> normalPullsExclusionList = [2500601, 422401, 306201, 399901, 399902, 399903, 399904, 201401, 301501, 112101, 313201, 319301, 319401, 320301, 422601, 426101, 328301, 328401, 235101, 235301, 136101, 339201, 140001, 140101, 140201, 580001, 580101, 580201, 581001, 581101, 581201, 582001, 582101, 582201, 583001, 583101, 583201, 583301, 190101, 290701]; // Add more IDs as needed

    protected override async Task HandleAsync()
    {
        ReqExecuteDailyFreeGacha req = await ReadData<ReqExecuteDailyFreeGacha>();

        // Count determines whether we select 1 or 10 characters
        int numberOfPulls = 1;

        int bannerID = req.GachaId;
        Logging.WriteLine($"Event ID: {req.EventId}" );
        Logging.WriteLine($"Banner ID: {bannerID}");
        GachaTypeRecord gachaType = GameData.Instance.gachaTypes[bannerID];

        //Get the banner ID and load the banner data from GachaTypeTable
        User user = GetUser();

        ResExecuteDailyFreeGacha response = new();

        Logging.WriteLine($"Currency type: Daily free pull");

        List<CharacterRecord> selectedCharacters = GachaUtils.ExecuteGachaPull(gachaType, numberOfPulls, user);
        
        List<Tuple<int, int>> pieceIds = []; // 2D array to store characterId and pieceId as Tuple
                                             // Add each character's item to user.Items if the character exists in user.Characters
        foreach (CharacterRecord characterData in selectedCharacters)
        {
            // Check if the item for this character already exists in user.Items based on ItemType
            DbItemData? existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.PieceId);

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
                    Lv = existingItem.Level,
                    Exp = existingItem.Exp,
                    Position = existingItem.Position,
                    Isn = existingItem.Isn
                });
            }
            else
            {
                // If the item does not exist, create a new item entry
                DbItemData newItem = new()
                {
                    ItemType = characterData.PieceId,
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
                    Lv = newItem.Level,
                    Exp = newItem.Exp,
                    Position = newItem.Position,
                    Isn = newItem.Isn
                });
            }
        }

        // Populate the 2D array with characterId and pieceId for each selected character
        foreach (CharacterRecord characterData in selectedCharacters)
        {
            int characterId = characterData.Id;
            int pieceId = characterData.PieceId;

            // Store characterId and pieceId in the array
            pieceIds.Add(Tuple.Create(characterId, pieceId));
            int Id = user.GenerateUniqueCharacterId();
            response.Gacha.Add(new NetGachaEntityData()
            {
                Corporation = 1,
                PieceCount = 1,
                CurrencyValue = 5,
                Sn = Id,
                Tid = characterId,
                Type = 1
            });

            // Check if the user already has the character, if not add it
            if (!user.HasCharacter(characterId))
            {
                response.Characters.Add(new NetUserCharacterDefaultData()
                {
                    CostumeId = 0,
                    Csn = Id,
                    Grade = 0,
                    Lv = 1,
                    Skill1Lv = 1,
                    Skill2Lv = 1,
                    Tid = characterId,
                    UltiSkillLv = 1
                });

                user.Characters.Add(new CharacterModel()
                {
                    CostumeId = 0,
                    Csn = Id,
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = characterId,
                    UltimateLevel = 1
                });
            }
        }
        user.GachaDailyFreePulls.Add(req.EventId);

        user.AddGachaPullCount(bannerID, numberOfPulls);
        
        JsonDb.Save();

        await WriteDataAsync(response);
    }
   
}