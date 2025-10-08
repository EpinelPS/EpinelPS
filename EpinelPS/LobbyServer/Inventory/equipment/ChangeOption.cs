using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/changeoption")]
    public class ChangeOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningChangeOption req = await ReadData<ReqAwakeningChangeOption>();
            User user = GetUser();

            ResAwakeningChangeOption response = new ResAwakeningChangeOption();

            if (req.Isn <= 0)
            {
                await WriteDataAsync(response);
                return;
            }

            EquipmentAwakeningData? oldAwakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn && !x.IsNewData);

            if (oldAwakening == null)
            {
                oldAwakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
                await WriteDataAsync(response);
                return;
            }

            List<EquipmentAwakeningData> duplicates = user.EquipmentAwakenings.Where(x => x.Isn == req.Isn).ToList();

            if (req.IsChanged)
            {
                if (duplicates.Count > 1)
                {
                    List<EquipmentAwakeningData> oldEntries = duplicates.Where(x => !x.IsNewData).ToList();
                    foreach (EquipmentAwakeningData oldEntry in oldEntries)
                    {
                        user.EquipmentAwakenings.Remove(oldEntry);
                    }

                    EquipmentAwakeningData? newEntry = duplicates.FirstOrDefault(x => x.IsNewData);
                    if (newEntry != null)
                    {
                        newEntry.IsNewData = false;
                    }
                }
                else if (duplicates.Count == 1)
                {
                    EquipmentAwakeningData singleEntry = duplicates[0];
                    if (singleEntry.IsNewData)
                    {
                        singleEntry.IsNewData = false;
                    }
                }

                EquipmentAwakeningData? confirmedAwakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
                if (confirmedAwakening != null)
                {
                    response.Awakening = new NetEquipmentAwakening()
                    {
                        Isn = confirmedAwakening.Isn,
                        Option = new NetEquipmentAwakeningOption()
                        {
                            Option1Id = confirmedAwakening.Option.Option1Id,
                            Option1Lock = confirmedAwakening.Option.Option1Lock,
                            IsOption1DisposableLock = confirmedAwakening.Option.IsOption1DisposableLock,
                            Option2Id = confirmedAwakening.Option.Option2Id,
                            Option2Lock = confirmedAwakening.Option.Option2Lock,
                            IsOption2DisposableLock = confirmedAwakening.Option.IsOption2DisposableLock,
                            Option3Id = confirmedAwakening.Option.Option3Id,
                            Option3Lock = confirmedAwakening.Option.Option3Lock,
                            IsOption3DisposableLock = confirmedAwakening.Option.IsOption3DisposableLock
                        }
                    };
                }
            }
            else
            {
                if (duplicates.Count > 1)
                {
                    List<EquipmentAwakeningData> newEntries = duplicates.Where(x => x.IsNewData).ToList();
                    foreach (EquipmentAwakeningData newEntry in newEntries)
                    {
                        user.EquipmentAwakenings.Remove(newEntry);
                    }
                }

                EquipmentAwakeningData? originalAwakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
                if (originalAwakening != null)
                {
                    response.Awakening = new NetEquipmentAwakening()
                    {
                        Isn = originalAwakening.Isn,
                        Option = new NetEquipmentAwakeningOption()
                        {
                            Option1Id = originalAwakening.Option.Option1Id,
                            Option1Lock = originalAwakening.Option.Option1Lock,
                            IsOption1DisposableLock = originalAwakening.Option.IsOption1DisposableLock,
                            Option2Id = originalAwakening.Option.Option2Id,
                            Option2Lock = originalAwakening.Option.Option2Lock,
                            IsOption2DisposableLock = originalAwakening.Option.IsOption2DisposableLock,
                            Option3Id = originalAwakening.Option.Option3Id,
                            Option3Lock = originalAwakening.Option.Option3Lock,
                            IsOption3DisposableLock = originalAwakening.Option.IsOption3DisposableLock
                        }
                    };
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}