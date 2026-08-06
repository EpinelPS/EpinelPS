using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/usepiece")]
public class UsePiece : LobbyMessage
{
    private static readonly Random random = new();

    protected override async Task HandleAsync()
    {
        // TODO: If this process takes too long, consIder to avoId using function chain.
        /*
         * Req Contains:
         * Isn: long value, the item serial number of the piece
         * Count: int value, how many time
         */
        ReqUsePiece req = await ReadData<ReqUsePiece>();
        User user = GetUser();
        ResUsePiece response = new();

        DbItemData piece = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new InvalidDataException("cannot find piece with isn " + req.Isn);
        if (req.Count * 50 > piece.Count) throw new Exception("count mismatch");

        piece.Count -= req.Count * 50;
        if (piece.Count == 0) user.Items.Remove(piece);

        ItemPieceRecord? pItem = GameData.Instance.PieceItems
            .FirstOrDefault(x => x.Value.Id == piece.ItemType).Value
            ?? throw new Exception("cannot find piece Id " + piece.ItemType);

        // Load the character probability list for the mold
        IEnumerable<GachaListProbRecord> probList = GameData.Instance.GachaGradeProb
            .Where(gradeProb => gradeProb.Key == pItem.UseId)
            .SelectMany(grade => GameData.Instance.GachaListProb.Where(list => list.Value.GroupId == grade.Value.GachaListId))
            .Select(i => i.Value);

        NetRewardData reward = new();
        IEnumerable<CharacterRecord> selectedCharacters = Enumerable.Range(1, req.Count)
            .Select(_ => SelectRandomCharacter(probList));

        int totalBodyLabels = 0;
        foreach (CharacterRecord? character in selectedCharacters)
        {
            DbItemData? spareItem = user.Items.FirstOrDefault(i => i.ItemType == character.PieceId);

            if (user.GetCharacter(character.Id) is CharacterModel ownedCharacter)
            {
                // If the character already exists, we can increase its piece count
                int maxLimitBroken = GetValueByRarity(character.OriginalRare, 0, 2, 11);
                bool canIncreaseItem = character.OriginalRare != OriginalRareType.R && ownedCharacter.Grade + (spareItem?.Count ?? 0) < maxLimitBroken;
                (int newSpareItemCount, int dissoluteCharacterCount) = canIncreaseItem ? (1, 0) : (0, 1);
                if (canIncreaseItem)
                {
                    if (spareItem != null)
                    {
                        spareItem.Count = newSpareItemCount;
                    }
                    else
                    {
                        spareItem = new()
                        {
                            ItemType = character.PieceId,
                            Csn = 0,
                            Count = newSpareItemCount,
                            Level = 0,
                            Exp = 0,
                            Position = 0,
                            Corp = 0,
                            Isn = user.GenerateUniqueItemId()
                        };
                        user.Items.Add(spareItem);
                    }

                    reward.UserItems.Add(NetUtils.UserItemDataToNet(spareItem));
                    reward.Character.Add(GetNetCharacterData(ownedCharacter));
                }
                else
                {
                    // If we cannot increase the item, we give body label instead
                    int bodyLabel = GetValueByRarity(character.OriginalRare, 150, 200, 6000);
                    totalBodyLabels += bodyLabel * dissoluteCharacterCount;
                    reward.Character.Add(GetNetCharacterData(ownedCharacter, bodyLabel));
                }
            }
            else
            {
                int csn = user.GenerateUniqueCharacterId();
                reward.UserCharacters.Add(new NetUserCharacterDefaultData
                {
                    CostumeId = 0,
                    Csn = csn,
                    Grade = 0,
                    Lv = 1,
                    Skill1Lv = 1,
                    Skill2Lv = 1,
                    Tid = character.Id,
                    UltiSkillLv = 1
                });
                reward.Character.Add(new NetCharacterData
                {
                    Csn = user.GenerateUniqueCharacterId(),
                    Tid = character.Id,
                });
                user.Characters.Add(new CharacterModel
                {
                    CostumeId = 0,
                    Csn = csn,
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = character.Id,
                    UltimateLevel = 1
                });

                // Add "New Character" Badge
                user.AddBadge(BadgeContents.NikkeNew, character.NameCode.ToString());
                user.AddTrigger(Trigger.ObtainCharacter, 1, character.NameCode);
                if (character.OriginalRare == OriginalRareType.SR)
                {
                    user.AddTrigger(Trigger.ObtainCharacterSSR, 1);
                }
                else
                {
                    user.AddTrigger(Trigger.ObtainCharacterNew, 1);
                }

                if (character.OriginalRare == OriginalRareType.SSR || character.OriginalRare == OriginalRareType.SR)
                {
                    user.BondInfo.Add(new() { NameCode = character.NameCode, Lv = 1 });
                }
            }

            user.AddTrigger(Trigger.GachaCharacter, 0, 0);
        }

        reward.Currency.Add(new NetCurrencyData() { Type = (int)CurrencyType.DissolutionPoint, Value = totalBodyLabels });
        user.AddCurrency(CurrencyType.DissolutionPoint, totalBodyLabels);
        reward.UserItems.Add(NetUtils.UserItemDataToNet(piece));

        response.Reward = reward;

        JsonDb.Save();

        await WriteDataAsync(response);
    }

    private CharacterRecord SelectRandomCharacter(IEnumerable<GachaListProbRecord> charProbs)
    {
        // Changed this to use the Gacha list probability table instead since it contains the probabilities without the need for splitting the groups.
        // This should work no matter how many character are added and no matter what their probabilities are.

        // Create the probability table
        int maxCharProb = 0;

        Dictionary<int, (int minProbInc, int maxProbEx)> charProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        foreach(GachaListProbRecord charProb in charProbs){
            charProbsTable.Add(charProb.Id, new (maxCharProb, maxCharProb + charProb.Prob));
            maxCharProb += charProb.Prob;
        }

        // Now, do the pull
        int charRoll = (int)random.NextInt64(maxCharProb);

        GachaListProbRecord selectedCharacter = charProbs.Where(charP => charP.Id == charProbsTable.Where( p => charRoll >= p.Value.minProbInc && charRoll < p.Value.maxProbEx).Select( p=> p.Key).First()).First();

        return GameData.Instance.CharacterTable[selectedCharacter.GachaId];
    }

    private int GetValueByRarity(OriginalRareType rarity, int rValue, int srValue, int ssrValue) => rarity switch
    {
        OriginalRareType.R => rValue,
        OriginalRareType.SR => srValue,
        OriginalRareType.SSR => ssrValue,
        _ => throw new Exception($"Unknown character rarity: {rarity}")
    };

    private NetCharacterData GetNetCharacterData(CharacterModel character, int bodyLabel = 0)
    {
        return new NetCharacterData
        {
            Csn = character.Csn,
            Tid = character.Tid,
            PieceCount = bodyLabel == 0 ? 1 : 0,
            CurrencyValue = bodyLabel
        };
    }
}

internal record PieceGradeProb(double RProb, double SRProb, double SSRProb);
