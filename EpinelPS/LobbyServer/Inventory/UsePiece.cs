using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/usepiece")]
    public class UsePiece : LobbyMsgHandler
    {
        private static readonly Random random = new Random();

        protected override async Task HandleAsync()
        {
            // TODO: If this process takes too long, consider to avoid using function chain.
            /*
             * Req Contains:
             * Isn: long value, the item serial number of the piece
             * Count: int value, how many time
             */
            var req = await ReadData<ReqUsePiece>();
            var user = GetUser();
            var response = new ResUsePiece();

            var piece = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new InvalidDataException("cannot find piece with isn " + req.Isn);
            if (req.Count * 50 > piece.Count) throw new Exception("count mismatch");

            piece.Count -= req.Count * 50;
            if (piece.Count == 0) user.Items.Remove(piece);

            ItemPieceRecord? pItem = GameData.Instance.PieceItems
                .FirstOrDefault(x => x.Value.id == piece.ItemType).Value
                ?? throw new Exception("cannot find piece id " + piece.ItemType);

            var probList = GameData.Instance.GachaGradeProb
                .Where(x => x.Key == pItem.use_id)
                .SelectMany(grade => GameData.Instance.GachaListProb.Where(list => list.Value.group_id == grade.Value.gacha_list_id))
                .Select(i => i.Value);
            var allCharacters = probList.SelectMany(e => GameData.Instance.CharacterTable.Values.Where(c => c.id == e.gacha_id));

            NetRewardData reward = new();
            var selectedCharacterGroups = Enumerable.Range(1, req.Count)
                .Select(_ => SelectRandomCharacter(allCharacters, pItem.id))
                .GroupBy(c => c.name_code);
            var selectedCharacters = Enumerable.Range(1, req.Count)
                .Select(_ => SelectRandomCharacter(allCharacters, pItem.id));

            int totalBodyLabels = 0;
            foreach (var character in selectedCharacters)
            {
                ItemData? spareItem = user.Items.FirstOrDefault(i => i.ItemType == character.piece_id);

                if (user.GetCharacter(character.id) is Database.Character ownedCharacter)
                {
                    // If the character already exists, we can increase its piece count
                    int maxLimitBroken = GetValueByRarity(character.original_rare, 0, 2, 11);
                    bool canIncreaseItem = character.original_rare != "R" && ownedCharacter.Grade + (spareItem?.Count ?? 0) < maxLimitBroken;
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
                                ItemType = character.piece_id,
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
                        int bodyLabel = GetValueByRarity(character.original_rare, 150, 200, 6000);
                        totalBodyLabels += bodyLabel * dissoluteCharacterCount;
                        reward.Character.Add(GetNetCharacterData(ownedCharacter, bodyLabel));
                    }
                }
                else
                {
                    var csn = user.GenerateUniqueCharacterId();
                    reward.UserCharacters.Add(new NetUserCharacterDefaultData
                    {
                        CostumeId = 0,
                        Csn = csn,
                        Grade = 0,
                        Lv = 1,
                        Skill1Lv = 1,
                        Skill2Lv = 1,
                        Tid = character.id,
                        UltiSkillLv = 1
                    });
                    reward.Character.Add(new NetCharacterData
                    {
                        Csn = user.GenerateUniqueCharacterId(),
                        Tid = character.id,
                    });
                    user.Characters.Add(new Database.Character
                    {
                        CostumeId = 0,
                        Csn = csn,
                        Grade = 0,
                        Level = 1,
                        Skill1Lvl = 1,
                        Skill2Lvl = 1,
                        Tid = character.id,
                        UltimateLevel = 1
                    });

                    // Add "New Character" Badge
                    user.AddBadge(BadgeContents.NikkeNew, character.name_code.ToString());
                    user.AddTrigger(TriggerType.ObtainCharacter, 1, character.name_code);
                    if (character.original_rare == "SSR")
                    {
                        user.AddTrigger(TriggerType.ObtainCharacterSSR, 1);
                    }
                    else
                    {
                        user.AddTrigger(TriggerType.ObtainCharacterNew, 1);
                    }

                    if (character.original_rare == "SSR" || character.original_rare == "SR")
                    {
                        user.BondInfo.Add(new() { NameCode = character.name_code, Lv = 1 });
                    }
                }

                user.AddTrigger(TriggerType.GachaCharacter, 0, 0);
            }

            reward.Currency.Add(new NetCurrencyData() { Type = (int)CurrencyType.DissolutionPoint, Value = totalBodyLabels });
            user.AddCurrency(CurrencyType.DissolutionPoint, totalBodyLabels);
            reward.UserItems.Add(NetUtils.UserItemDataToNet(piece));

            response.Reward = reward;

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private CharacterRecord SelectRandomCharacter(IEnumerable<CharacterRecord> characters, int pieceId)
        {
            var gradeProb = GetPieceGradeProb(pieceId);
            var rCharacters = characters.Where(c => c.original_rare == "R");
            var srCharacters = characters.Where(c => c.original_rare == "SR");
            var ssrCharacters = characters.Where(c => c.original_rare == "SSR");

            double roll = random.NextDouble() * 100;

            if (0.0 < gradeProb.rProb && roll < gradeProb.rProb && rCharacters.Any())
            {
                return rCharacters.ElementAt(random.Next(rCharacters.Count()));
            }
            else if (0.0 < gradeProb.srProb && roll < gradeProb.rProb + gradeProb.srProb && srCharacters.Any())
            {
                int randomValue = random.Next(srCharacters.Count());
                Console.WriteLine($"Randomized Value: {randomValue}, Base: {srCharacters.Count()}");
                return srCharacters.ElementAt(randomValue);
            }
            else if (0.0 < gradeProb.ssrProb && roll < gradeProb.rProb + gradeProb.srProb + gradeProb.ssrProb && ssrCharacters.Any())
            {
                int randomValue = random.Next(ssrCharacters.Count());
                Console.WriteLine($"Randomized Value: {randomValue}, Base: {ssrCharacters.Count()}");
                return ssrCharacters.ElementAt(randomValue);
            }
            else
            {
                throw new Exception("No characters available for the given value.");
            }
        }

        // TODO: find the file where the grade probability is stored.
        // TODO: Add Helm Mold and Laplace Mold
        private PieceGradeProb GetPieceGradeProb(int pieceId) => pieceId switch
        {
            5310301 => new PieceGradeProb(0.0, 38.9997, 61.0003), // High quality Mold
            5310302 or 5310303 or 5310304 or 5310305 => new PieceGradeProb(19.9998, 29.9997, 50.0005), // Manufacturer Mold
            5310306 or 5310307 or 5310308 or 5310309 => new PieceGradeProb(0.0, 0.0, 100.0), // New Commander Mold or Perfect Mold
            5330201 or 5359001 => new PieceGradeProb(0.0, 78.9993, 21.0007), // Mid quality Mold
            _ => throw new Exception("unknown piece id")
        };

        private int GetValueByRarity(string rarity, int rValue, int srValue, int ssrValue) => rarity switch
        {
            "R" => rValue,
            "SR" => srValue,
            "SSR" => ssrValue,
            _ => throw new Exception($"Unknown character rarity: {rarity}")
        };

        private NetCharacterData GetNetCharacterData(Database.Character character, int bodyLabel = 0)
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

    internal record PieceGradeProb(double rProb, double srProb, double ssrProb);
}
