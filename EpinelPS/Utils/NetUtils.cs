﻿using System.Collections;
using System.Collections.Generic;
using EpinelPS.Data;
using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;
using static Google.Rpc.Context.AttributeContext.Types;

namespace EpinelPS.Utils
{
    public class NetUtils
    {
        public static (User?, AccessToken?) GetUser(string tokToCheck)
        {
            if (string.IsNullOrEmpty(tokToCheck))
                throw new Exception("missing auth token");


            foreach (var tok in JsonDb.Instance.LauncherAccessTokens)
            {
                if (tok.Token == tokToCheck)
                {
                    var user = JsonDb.Instance.Users.Find(x => x.ID == tok.UserID);
                    if (user != null)
                    {
                        return (user, tok);
                    }
                }
            }

            return (null, null);
        }
        public static NetUserItemData ToNet(ItemData item)
        {
            return new()
            {
                Corporation = item.Corp,
                Count = item.Count,
                Csn = item.Csn,
                Exp = item.Exp,
                Isn = item.Isn,
                Lv = item.Level,
                Position = item.Position,
                Tid = item.ItemType
            };
        }

        internal static NetUserItemData UserItemDataToNet(ItemData item)
        {
            return new NetUserItemData()
            {
                Count = item.Count,
                Tid = item.ItemType,
                Csn = item.Csn,
                Lv = item.Level,
                Exp = item.Exp,
                Corporation = item.Corp,
                Isn = item.Isn,
                Position = item.Position
            };
        }


        public static List<NetUserItemData> GetUserItems(User user)
        {
            List<NetUserItemData> ret = new();
            Dictionary<int, NetUserItemData> itemDictionary = new Dictionary<int, NetUserItemData>();

            foreach (var item in user.Items.ToList())
            {
                if (item.Csn == 0)
                {
                    if (itemDictionary.ContainsKey(item.ItemType))
                    {
                        itemDictionary[item.ItemType].Count++;
                    }
                    else
                    {
                        itemDictionary[item.ItemType] = UserItemDataToNet(item);
                    }
                }
                else
                {
                    itemDictionary[item.ItemType] = UserItemDataToNet(item);
                }
            }

            return ret;
        }

        public static int GetItemPos(User user, long isn)
        {
            foreach (var item in user.Items)
            {
                if (item.Isn == isn)
                {
                    var subType = GameData.Instance.GetItemSubType(item.ItemType);
                    switch (subType)
                    {
                        case "Module_A":
                            return 0;
                        case "Module_B":
                            return 1;
                        case "Module_C":
                            return 2;
                        case "Module_D":
                            return 3;
                        default:
                            Console.WriteLine("Unknown item subtype: " + subType);
                            break;
                    }
                    break;
                }
            }

            return 0;
        }
        /// <summary>
        /// Takes multiple NetRewardData objects and merges it into one. Note that this function expects that rewards are already applied to user object.
        /// </summary>
        /// <param name="rewards">list of rewards</param>
        /// <param name="user">user to pull old currency count from</param>
        /// <returns></returns>
        public static NetRewardData MergeRewards(List<NetRewardData> rewards, User user)
        {
            NetRewardData result = new();

            Dictionary<int, long> currencyDict = [];
            List<NetItemData> items = [];

            foreach (NetRewardData reward in rewards)
            {
                foreach (var c in reward.Currency)
                {
                    if (currencyDict.ContainsKey(c.Type))
                        currencyDict[c.Type] += c.Value;
                    else
                        currencyDict.Add(c.Type, c.Value);
                }

                foreach (var item in reward.Item)
                {
                    items.Add(item);
                }

                foreach (var item in reward.UserItems)
                {
                    // TODO: do these need to be combined?
                    result.UserItems.Add(item);
                }

                foreach (var item in reward.Point)
                {
                    result.Point.Add(item);
                }

                foreach (var c in reward.Character)
                {
                    Logging.WriteLine("MergeRewards - TODO Character", LogType.Error);
                }

                if (reward.InfraCoreExp != null)
                    result.InfraCoreExp = reward.InfraCoreExp;
            }

            foreach (var c in currencyDict)
            {
                result.Currency.Add(new NetCurrencyData() { Value = c.Value, Type = c.Key, FinalValue = user.Currency[(CurrencyType)c.Key] });
            }

            // TODO is this right?
            foreach (var c in items)
            {
                result.Item.Add(c);
            }
            return result;
        }

        private static long CalcOutpostRewardAmount(int value, double ratio, double boost, double elapsedMinutes)
        {
            double baseValue = value * ratio / 10000.0;
            double minuteValue = baseValue + baseValue * boost / 100.0;


            return (long)Math.Floor(minuteValue * elapsedMinutes);
        }

        public static double CalculateBoostValueForOutpost(User user, CurrencyType type)
        {
            return user.OutpostBuffs.GetTotalPercentages(type) / 100.0;
        }

        public static long GetOutpostRewardAmount(User user, CurrencyType type, double mins, bool includeBoost)
        {
            var battleData = GameData.Instance.OutpostBattle[user.OutpostBattleLevel.Level];

            int value = 0;
            double ratio = 0;
            double boost = 1.0;
            if (includeBoost)
                boost += CalculateBoostValueForOutpost(user, type);

            switch (type)
            {
                case CurrencyType.CharacterExp2:
                    value = battleData.character_exp2;
                    ratio = 1;
                    break;
                case CurrencyType.CharacterExp:
                    value = battleData.character_exp1;
                    ratio = 3;
                    break;
                case CurrencyType.Gold:
                    value = battleData.credit;
                    ratio = 3;
                    break;
                case CurrencyType.UserExp:
                    value = battleData.user_exp;
                    ratio = 1;
                    break;
            }
            return CalcOutpostRewardAmount(value, ratio, boost, mins);
        }

        public static NetRewardData GetOutpostReward(User user, TimeSpan duration)
        {
            //duration = TimeSpan.FromHours(1);
            NetRewardData result = new();

            var battleData = GameData.Instance.OutpostBattle[user.OutpostBattleLevel.Level];

            result.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.CharacterExp2,
                FinalValue = 0,
                Value = CalcOutpostRewardAmount(battleData.character_exp2, 1, 1, duration.TotalMinutes)
            });

            result.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.CharacterExp,
                FinalValue = 0,
                Value = CalcOutpostRewardAmount(battleData.character_exp1, 3, 1, duration.TotalMinutes)
            });

            result.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.Gold,
                FinalValue = 0,
                Value = CalcOutpostRewardAmount(battleData.credit, 3, 1, duration.TotalMinutes)
            });

            result.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.UserExp,
                FinalValue = 0,
                Value = CalcOutpostRewardAmount(battleData.user_exp, 3, 1, duration.TotalMinutes)
            });

            return result;
        }

        public static void RegisterRewardsForUser(User user, NetRewardData rewardData)
        {
            foreach (var item in rewardData.Currency)
            {
                user.AddCurrency((CurrencyType)item.Type, item.Value);
            }

            // TODO: other things that are used by the function above
        }

        internal static List<NetTimeReward> GetOutpostTimeReward(User user)
        {
            List<NetTimeReward> res = new List<NetTimeReward>();

            // NetTimeRewardBuff
            // FunctionType: 1: value increase, 2: percentage increase
            // Tid: Outpost building ID

            var goldBuff = new NetTimeReward()
            {
                UseId = 1,
                ValuePerMinAfterBuff = GetOutpostRewardAmount(user, CurrencyType.Gold, 1, true) * 10000,
                ValuePerMinBeforeBuff = GetOutpostRewardAmount(user, CurrencyType.Gold, 1, false) * 10000
            };
            foreach (var item in user.OutpostBuffs.CreditPercentages)
            {
                goldBuff.Buffs.Add(new NetTimeRewardBuff() { Tid = 22401, FunctionType = 2, SourceType = OutpostBuffSourceType.TacticAcademy, Value = item });
            }


            var battleDataBuff = new NetTimeReward()
            {
                UseId = 2,
                ValuePerMinAfterBuff = GetOutpostRewardAmount(user, CurrencyType.CharacterExp, 1, true) * 10000,
                ValuePerMinBeforeBuff = GetOutpostRewardAmount(user, CurrencyType.CharacterExp, 1, false) * 10000
            };
            foreach (var item in user.OutpostBuffs.BattleDataPercentages)
            {
                battleDataBuff.Buffs.Add(new NetTimeRewardBuff() { Tid = 22401, FunctionType = 2, SourceType = OutpostBuffSourceType.TacticAcademy, Value = item });
            }

            var xpBuff = new NetTimeReward()
            {
                UseId = 3,
                ValuePerMinAfterBuff = GetOutpostRewardAmount(user, CurrencyType.UserExp, 1, true) * 10000,
                ValuePerMinBeforeBuff = GetOutpostRewardAmount(user, CurrencyType.UserExp, 1, false) * 10000
            };
            foreach (var item in user.OutpostBuffs.UserExpPercentages)
            {
                xpBuff.Buffs.Add(new NetTimeRewardBuff() { Tid = 22401, FunctionType = 2, SourceType = OutpostBuffSourceType.TacticAcademy, Value = item });
            }

            var coredustBuff = new NetTimeReward()
            {
                UseId = 4,
                ValuePerMinAfterBuff = GetOutpostRewardAmount(user, CurrencyType.CharacterExp2, 60, true) * 100,
                ValuePerMinBeforeBuff = GetOutpostRewardAmount(user, CurrencyType.CharacterExp2, 60, false) * 100
            };
            foreach (var item in user.OutpostBuffs.CoreDustPercentages)
            {
                coredustBuff.Buffs.Add(new NetTimeRewardBuff() { Tid = 22401, FunctionType = 2, SourceType = OutpostBuffSourceType.TacticAcademy, Value = item });
            }

            res.Add(battleDataBuff);
            res.Add(goldBuff);
            res.Add(xpBuff);
            res.Add(coredustBuff);

            return res;
        }

        private static NetWholeTeamSlot? LookupCharacter(User user, long csn, int slot)
        {
            if (csn == 0) return new() { Slot = slot };

            var result = new NetWholeTeamSlot();

            var c = user.GetCharacterBySerialNumber(csn);
            if (c == null) return new() { Slot = slot };

            return new NetWholeTeamSlot()
            {
                CostumeId = c.CostumeId,
                Csn = csn,
                Lv = c.Level,
                Slot = slot,
                Tid = c.Tid,
                //UserFavoriteItem: TODO
            };
        }

        internal static NetWholeUserTeamData GetDisplayedTeam(User user)
        {
            NetWholeUserTeamData result = new() { TeamNumber = 1, Type = 2 };

            result.Slots.Add(LookupCharacter(user, user.RepresentationTeamDataNew[0], 1));
            result.Slots.Add(LookupCharacter(user, user.RepresentationTeamDataNew[1], 2));
            result.Slots.Add(LookupCharacter(user, user.RepresentationTeamDataNew[2], 3));
            result.Slots.Add(LookupCharacter(user, user.RepresentationTeamDataNew[3], 4));
            result.Slots.Add(LookupCharacter(user, user.RepresentationTeamDataNew[4], 5));

            int totalCP = 0;

            foreach (var item in user.RepresentationTeamDataNew)
            {
                totalCP += FormulaUtils.CalculateCP(user, item);
            }

            result.TeamCombat = totalCP;

            return result;
        }

        public static NetRewardData UseLootBox(User user, int boxId, int count)
        {
            ItemConsumeRecord? cItem = GameData.Instance.ConsumableItems.Where(x => x.Value.id == boxId).FirstOrDefault().Value ?? throw new Exception("cannot find box id " + boxId);

            if (cItem.use_type != "ItemRandomBox") throw new Exception("expected random box");

            // find matching probability entries
            var probabilityEntries = GameData.Instance.RandomItem.Values.Where(x => x.group_id == cItem.use_id).ToArray();
            if (!probabilityEntries.Any()) throw new Exception($"cannot find any probability entries with ID {cItem.use_id}, box ID: {cItem.id}");

            // run probability as many times as needed
            NetRewardData ret = new() { PassPoint = new() };
            for (int i = 0; i < count; i++)
            {
                var winningRecord = Rng.PickWeightedItem(probabilityEntries);

                if (winningRecord.reward_value_min  != winningRecord.reward_value_max)
                {
                    Logging.WriteLine("TODO: reward_value_max", LogType.Warning);
                }

                if (winningRecord.reward_type == "Currency")
                    RewardUtils.AddSingleCurrencyObject(user, ref ret, (CurrencyType)winningRecord.reward_id, winningRecord.reward_value_min);
                else
                    RewardUtils.AddSingleObject(user, ref ret, winningRecord.reward_id, winningRecord.reward_type, winningRecord.reward_value_min);
            }
            JsonDb.Save();

            return ret;
        }
    }
}