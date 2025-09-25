using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/upgradeoption")]
    public class AwakeningUpgradeOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningUpgradeOption req = await ReadData<ReqAwakeningUpgradeOption>();
            User user = GetUser();

            ResAwakeningUpgradeOption response = new();

            ItemData item = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new Exception($"not Isn = {req.Isn}");
            response.Items.Add(NetUtils.ToNet(item));

            response.Currencies.Add(new NetUserCurrencyData()
            {
                Type = (int)CurrencyType.Gold,
                Value = user.Currency[CurrencyType.Gold]
            });

            AwakeningOption oldOption = new();
            foreach (var option in user.EquipmentAwakeningOptions)
            {
                if (req.Isn == option.Key)
                {
                    oldOption = option.Value;
                }
            }

            ResetAwakeningOption newOption = new()
            {
                Isn = req.Isn,
                Option1Id = oldOption.Option1Id,
                Option2Id = oldOption.Option2Id,
                Option3Id = oldOption.Option3Id,
                Option1Lock = oldOption.Option1Lock,
                Option2Lock = oldOption.Option2Lock,
                Option3Lock = oldOption.Option3Lock,
                IsOption1DisposableLock = oldOption.IsOption1DisposableLock,
                IsOption2DisposableLock = oldOption.IsOption2DisposableLock,
                IsOption3DisposableLock = oldOption.IsOption3DisposableLock,
            };

            int optionId = GetOptionId(oldOption);
            if (!oldOption.Option1Lock && !oldOption.IsOption1DisposableLock)
            {
                newOption.Option1Id = optionId;
            }
            else if (!oldOption.Option2Lock && !oldOption.IsOption2DisposableLock)
            {
                newOption.Option2Id = optionId;
            }
            else if (!oldOption.Option3Lock && !oldOption.IsOption3DisposableLock)
            {
                newOption.Option3Id = optionId;
            }
            user.ResetAwakeningOption = newOption;
            if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out _))
            {
                user.EquipmentAwakeningOptions[req.Isn].IsOption1DisposableLock = false;
                user.EquipmentAwakeningOptions[req.Isn].IsOption2DisposableLock = false;
                user.EquipmentAwakeningOptions[req.Isn].IsOption3DisposableLock = false;
            }
            response.ResetOption = NetUtils.AwakeningOptionToNet(newOption);

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private static int GetOptionId(AwakeningOption option)
        {
            List<EquipmentOptionRecord> optionDic = GetOptionDic(option);

            List<int> stateEffectIds = [];
            foreach (var record in optionDic)
            {
                foreach (StateEffect effect in record.state_effect_list)
                {
                    stateEffectIds.Add(effect.state_effect_id);
                }
            }

            var random = new Random();
            int index = random.Next(stateEffectIds.Count); // 随机生成索引
            return stateEffectIds[index];
        }

        private static List<EquipmentOptionRecord> GetOptionDic(AwakeningOption option)
        {

            // 获取未锁定的选项
            if (!option.Option1Lock && !option.IsOption1DisposableLock)
            {
                return [.. GameData.Instance.EquipmentOptionTable.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option1Id)).Select(x => x.Value)];
            }
            else if (!option.Option2Lock && !option.IsOption2DisposableLock)
            {
                return [.. GameData.Instance.EquipmentOptionTable.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option2Id)).Select(x => x.Value)];
            }
            else if (!option.Option3Lock && !option.IsOption3DisposableLock)
            {
                return [.. GameData.Instance.EquipmentOptionTable.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option3Id)).Select(x => x.Value)];
            }
            return [];
        }

        private static int GetStateEffectGroupid(int optionId)
        {
            foreach (var record in GameData.Instance.EquipmentOptionTable)
            {
                foreach (StateEffect effect in record.Value.state_effect_list)
                {
                    if (effect.state_effect_id == optionId)
                    {
                        return record.Value.state_effect_group_id;
                    }
                }
            }
            return 0;
        }

    }
}