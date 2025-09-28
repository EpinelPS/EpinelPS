using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/usepiece")]
    public class UsePiece : LobbyMsgHandler
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

            ItemData piece = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new InvalidDataException("cannot find piece with isn " + req.Isn);
            if (req.Count * 50 > piece.Count) throw new Exception("count mismatch");

            piece.Count -= req.Count * 50;
            if (piece.Count == 0) user.Items.Remove(piece);

            ItemPieceRecord? pItem = GameData.Instance.PieceItems
                .FirstOrDefault(x => x.Value.Id == piece.ItemType).Value
                ?? throw new Exception("cannot find piece Id " + piece.ItemType);

            IEnumerable<GachaListProbRecord> probList = GameData.Instance.GachaGradeProb
                .Where(x => x.Key == pItem.UseId)
                .SelectMany(grade => GameData.Instance.GachaListProb.Where(list => list.Value.GroupId == grade.Value.GachaListId))
                .Select(i => i.Value);
            IEnumerable<CharacterRecord> allCharacters = probList.SelectMany(e => GameData.Instance.CharacterTable.Values.Where(c => c.Id == e.GachaId));

            NetRewardData reward = new();
            IEnumerable<CharacterRecord> selectedCharacters = Enumerable.Range(1, req.Count)
                .Select(_ => SelectRandomCharacter(allCharacters, pItem.Id));

            int totalBodyLabels = 0;
            foreach (CharacterRecord? character in selectedCharacters)
            {
                ItemData? spareItem = user.Items.FirstOrDefault(i => i.ItemType == character.PieceId);

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

        private CharacterRecord SelectRandomCharacter(IEnumerable<CharacterRecord> characters, int pieceId)
        {
            PieceGradeProb gradeProb = GetPieceGradeProb(pieceId);
            IEnumerable<CharacterRecord> rCharacters = characters.Where(c => c.OriginalRare == OriginalRareType.R);
            IEnumerable<CharacterRecord> srCharacters = characters.Where(c => c.OriginalRare == OriginalRareType.SR);
            IEnumerable<CharacterRecord> ssrCharacters = characters.Where(c => c.OriginalRare == OriginalRareType.SSR);

            double roll = random.NextDouble() * 100;

            if (0.0 < gradeProb.RProb && roll < gradeProb.RProb && rCharacters.Any())
            {
                return rCharacters.ElementAt(random.Next(rCharacters.Count()));
            }
            else if (0.0 < gradeProb.SRProb && roll < gradeProb.RProb + gradeProb.SRProb && srCharacters.Any())
            {
                return srCharacters.ElementAt(random.Next(srCharacters.Count()));
            }
            else if (0.0 < gradeProb.SSRProb && roll < gradeProb.RProb + gradeProb.SRProb + gradeProb.SSRProb && ssrCharacters.Any())
            {
                return ssrCharacters.ElementAt(random.Next(ssrCharacters.Count()));
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
            5330201 or 5359001 => new PieceGradeProb(0.0, 78.9993, 21.0007), // MId quality Mold
            _ => throw new Exception("unknown piece Id")
        };

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
}
