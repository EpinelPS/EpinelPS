using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/getawakeningdetail")]
    public class GetAwakeningDetail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetAwakeningDetail req = await ReadData<ReqGetAwakeningDetail>();
            User user = GetUser();

            ResGetAwakeningDetail response = new ResGetAwakeningDetail();

            // Validate input parameters
            if (req.Isn <= 0)
            {
                await WriteDataAsync(response);
                return;
            }

            // Find the equipment awakening data
            EquipmentAwakeningData? awakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
            if (awakening == null)
            {
                await WriteDataAsync(response);
                return;
            }

            // Set current options
            response.CurrentOption = new NetEquipmentAwakeningOption()
            {
                Option1Id = awakening.Option.Option1Id,
                Option1Lock = awakening.Option.Option1Lock,
                IsOption1DisposableLock = awakening.Option.IsOption1DisposableLock,
                Option2Id = awakening.Option.Option2Id,
                Option2Lock = awakening.Option.Option2Lock,
                IsOption2DisposableLock = awakening.Option.IsOption2DisposableLock,
                Option3Id = awakening.Option.Option3Id,
                Option3Lock = awakening.Option.Option3Lock,
                IsOption3DisposableLock = awakening.Option.IsOption3DisposableLock
            };

            NetEquipmentAwakeningOption newOption = new NetEquipmentAwakeningOption();

            // Process each option slot (1, 2, 3)
            for (int i = 1; i <= 3; i++)
            {
                // Get current option ID for this slot
                int currentOptionId = GetOptionIdForSlot(awakening.Option, i);
                bool isLocked = IsOptionLocked(awakening.Option, i);
                bool isDisposableLocked = IsOptionDisposableLocked(awakening.Option, i);

                // If option is permanently locked or disposable locked, keep it unchanged
                if (isLocked || isDisposableLocked)
                {
                    // Keep the current option unchanged
                    SetOptionForSlot(newOption, i, currentOptionId, isLocked, isDisposableLocked);
                    continue;
                }

                // If not locked, generate a new option
                int newOptionId = GenerateNewOptionId(currentOptionId);
                SetOptionForSlot(newOption, i, newOptionId, false, false);
            }

            response.NewOption = newOption;

            await WriteDataAsync(response);
        }

        private int GetOptionIdForSlot(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.Option1Id,
                2 => option.Option2Id,
                3 => option.Option3Id,
                _ => 0
            };
        }

        private bool IsOptionLocked(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.Option1Lock,
                2 => option.Option2Lock,
                3 => option.Option3Lock,
                _ => false
            };
        }

        private bool IsOptionDisposableLocked(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.IsOption1DisposableLock,
                2 => option.IsOption2DisposableLock,
                3 => option.IsOption3DisposableLock,
                _ => false
            };
        }

        private void SetOptionForSlot(NetEquipmentAwakeningOption option, int slot, int optionId, bool locked, bool disposableLocked)
        {
            switch (slot)
            {
                case 1:
                    option.Option1Id = optionId;
                    option.Option1Lock = locked;
                    option.IsOption1DisposableLock = disposableLocked;
                    break;
                case 2:
                    option.Option2Id = optionId;
                    option.Option2Lock = locked;
                    option.IsOption2DisposableLock = disposableLocked;
                    break;
                case 3:
                    option.Option3Id = optionId;
                    option.Option3Lock = locked;
                    option.IsOption3DisposableLock = disposableLocked;
                    break;
            }
        }

        private int GenerateNewOptionId(int currentOptionId)
        {
            // Get the current option record
            if (!GameData.Instance.EquipmentOptionTable.TryGetValue(currentOptionId, out EquipmentOptionRecord? currentOption))
            {
                return currentOptionId;
            }

            // Get the group ID of the current option
            int groupId = currentOption.EquipmentOptionGroupId;

            // Find all options in the same group
            List<EquipmentOptionRecord> optionsInGroup = GameData.Instance.EquipmentOptionTable.Values
                .Where(x => x.EquipmentOptionGroupId == groupId)
                .ToList();

            if (optionsInGroup.Count == 0)
            {
                return currentOptionId;
            }

            // Calculate total ratio for probability calculation
            long totalRatio = optionsInGroup.Sum(x => (long)x.OptionRatio);

            if (totalRatio == 0)
            {
                return currentOptionId;
            }

            // Select a new option based on probability
            Random random = new Random();
            long randomValue = random.NextInt64(0, totalRatio);
            long cumulativeRatio = 0;

            foreach (EquipmentOptionRecord option in optionsInGroup)
            {
                cumulativeRatio += option.OptionRatio;
                if (randomValue < cumulativeRatio)
                {
                    return option.Id;
                }
            }

            return currentOptionId;
        }
    }
}