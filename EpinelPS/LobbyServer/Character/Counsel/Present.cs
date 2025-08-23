using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel
{
    [PacketPath("/character/attractive/present")]
    public class Present : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCharacterPresent req = await ReadData<ReqCharacterPresent>();
            User user = GetUser();

            ResCharacterPresent response = new ResCharacterPresent();

            NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);
            if (bondInfo == null)
            {
                return;
            }

            int totalExpGained = 0;
            CharacterRecord? characterRecord = GameData.Instance.CharacterTable.Values.FirstOrDefault(x => x.name_code == req.NameCode);

            foreach (NetItemData item in req.Items)
            {
                ItemMaterialRecord? materialInfo = GameData.Instance.itemMaterialTable.GetValueOrDefault(item.Tid);
                if (materialInfo != null && materialInfo.item_sub_type == "AttractiveMaterial")
                {
                    int expGained = materialInfo.item_value;

                    if (characterRecord != null)
                    {
                        if (materialInfo.material_type == "Corporation")
                        {
                            string corporation = materialInfo.name_localkey.Split('_')[2];
                            if (corporation.Equals(characterRecord.corporation, StringComparison.OrdinalIgnoreCase))
                            {
                                expGained *= 5;
                            }
                        }
                        else if (materialInfo.material_type == "Squad")
                        {
                            string squad = materialInfo.name_localkey.Split('_')[2];
                            if (squad.Equals(characterRecord.squad, StringComparison.OrdinalIgnoreCase))
                            {
                                expGained *= 3;
                            }
                        }
                    }

                    totalExpGained += expGained;

                    ItemData? userItem = user.Items.FirstOrDefault(x => x.ItemType == item.Tid);
                    if (userItem != null)
                    {
                        userItem.Count -= (int)item.Count;
                        if (userItem.Count <= 0)
                        {
                            user.Items.Remove(userItem);
                        }
                    }
                }
            }

            int beforeLv = bondInfo.Lv;
            int beforeExp = bondInfo.Exp;

            bondInfo.Exp += totalExpGained;
            UpdateAttractiveLevel(bondInfo);

            response.Attractive = bondInfo;
            response.Exp = new NetIncreaseExpData
            {
                NameCode = bondInfo.NameCode,
                BeforeLv = beforeLv,
                BeforeExp = beforeExp,
                CurrentLv = bondInfo.Lv,
                CurrentExp = bondInfo.Exp,
                GainExp = totalExpGained
            };

            response.Items.AddRange(NetUtils.GetUserItems(user));

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private void UpdateAttractiveLevel(NetUserAttractiveData attractiveData)
        {
            while (attractiveData.Lv < 40)
            {
                AttractiveLevelRecord? levelInfo = GameData.Instance.AttractiveLevelTable.Values.FirstOrDefault(x => x.attractive_level == attractiveData.Lv);

                if (levelInfo == null)
                {
                    break;
                }

                if (attractiveData.Exp >= levelInfo.attractive_point)
                {
                    attractiveData.Exp -= levelInfo.attractive_point;
                    attractiveData.Lv++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}