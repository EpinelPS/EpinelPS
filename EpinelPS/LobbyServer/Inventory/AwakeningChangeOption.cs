using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/changeoption")]
    public class AwakeningChangeOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningChangeOption req = await ReadData<ReqAwakeningChangeOption>();
            User user = GetUser();

            ResAwakeningChangeOption response = new();

            if (req.IsChanged)
            {
                // 效果变更
                ResetAwakeningOption reset = user.ResetAwakeningOption;
                AwakeningOption option = new()
                {
                    Option1Id = reset.Option1Id,
                    Option2Id = reset.Option2Id,
                    Option3Id = reset.Option3Id,
                    Option1Lock = reset.Option1Lock,
                    Option2Lock = reset.Option2Lock,
                    Option3Lock = reset.Option3Lock,
                    IsOption1DisposableLock = false,
                    IsOption2DisposableLock = false,
                    IsOption3DisposableLock = false
                };
                if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out _))
                {
                    user.EquipmentAwakeningOptions[req.Isn] = option;
                    response.Awakening = new NetEquipmentAwakening()
                    {
                        Isn = req.Isn,
                        Option = NetUtils.AwakeningOptionToNet(reset)
                    };
                }
                else
                {
                    // 手动添加装备没有觉醒数据
                    if (user.ResetAwakeningOption.Isn > 0)
                    {
                        user.EquipmentAwakeningOptions.Add(reset.Isn, option);
                        response.Awakening = new NetEquipmentAwakening()
                        {
                            Isn = req.Isn,
                            Option = NetUtils.AwakeningOptionToNet(reset)
                        };
                    }
                }
            }
            else
            {
                // 效果维持
                if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out AwakeningOption? awakening))
                {
                    response.Awakening = new NetEquipmentAwakening()
                    {
                        Isn = req.Isn,
                        Option = new()
                        {
                            Option1Id = awakening.Option1Id,
                            Option2Id = awakening.Option2Id,
                            Option3Id = awakening.Option3Id,
                            Option1Lock = awakening.Option1Lock,
                            Option2Lock = awakening.Option2Lock,
                            Option3Lock = awakening.Option3Lock,
                            IsOption1DisposableLock = awakening.IsOption1DisposableLock,
                            IsOption2DisposableLock = awakening.IsOption2DisposableLock,
                            IsOption3DisposableLock = awakening.IsOption3DisposableLock
                        }
                    };
                }
                else
                {
                    // 手动添加装备没有经过觉醒数据
                    response.Awakening = new NetEquipmentAwakening()
                    {
                        Isn = req.Isn,
                        Option = new NetEquipmentAwakeningOption() { }
                    };
                }
            }
            user.ResetAwakeningOption = new() { };
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}