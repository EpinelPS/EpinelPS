using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/resetoption")]
    public class AwakeningResetOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningResetOption req = await ReadData<ReqAwakeningResetOption>();
            User user = GetUser();

            ResAwakeningResetOption response = new();

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

            if (!oldOption.Option1Lock && !oldOption.IsOption1DisposableLock)
            {
                newOption.Option1Id = GetOptionId(oldOption, 1);
            }
            else if (!oldOption.Option2Lock && !oldOption.IsOption2DisposableLock)
            {
                newOption.Option2Id = GetOptionId(oldOption, 2);
            }
            else if (!oldOption.Option3Lock && !oldOption.IsOption3DisposableLock)
            {
                newOption.Option3Id = GetOptionId(oldOption, 3);
            }
            if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out _))
            {
                user.EquipmentAwakeningOptions[req.Isn].IsOption1DisposableLock = false;
                user.EquipmentAwakeningOptions[req.Isn].IsOption2DisposableLock = false;
                user.EquipmentAwakeningOptions[req.Isn].IsOption3DisposableLock = false;
            }
            user.ResetAwakeningOption = newOption;
            response.ResetOption = NetUtils.AwakeningOptionToNet(newOption);

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private static int GetOptionId(AwakeningOption option, int keyId)
        {

            Dictionary<int, EquipmentOptionRecord> optionDic = GameData.Instance.EquipmentOptionTable;
            List<int> lockKeys = GetLockKeys(option, keyId);

            List<int> stateEffectIds = [];
            foreach (var record in optionDic)
            {
                if (!lockKeys.Exists(x => x == record.Key))
                {
                    foreach (StateEffect effect in record.Value.state_effect_list)
                    {
                        stateEffectIds.Add(effect.state_effect_id);
                    }
                }
            }

            var random = new Random();
            int index = random.Next(stateEffectIds.Count); // 随机生成索引
            return stateEffectIds[index];
        }

        private static List<int> GetLockKeys(AwakeningOption option, int keyId)
        {
            List<int> lockKeys = [];
            Dictionary<int, EquipmentOptionRecord> dic = GameData.Instance.EquipmentOptionTable;
            // if (option.Option1Lock || option.IsOption1DisposableLock)
            if (keyId != 1)
            {
                lockKeys.AddRange([.. dic.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option1Id)).Select(x => x.Key)]);
            }
            // if (option.Option2Lock || option.IsOption2DisposableLock)
            if (keyId != 2)
            {
                lockKeys.AddRange([.. dic.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option2Id)).Select(x => x.Key)]);
            }
            // if (option.Option3Lock || option.IsOption3DisposableLock)
            if (keyId != 3)
            {
                lockKeys.AddRange([.. dic.Where(x => x.Value.state_effect_group_id == GetStateEffectGroupid(option.Option3Id)).Select(x => x.Key)]);
            }
            return [.. lockKeys.Distinct()];
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