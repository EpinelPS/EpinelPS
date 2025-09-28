using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/managementharmonycube")]
    public class ManagementHarmonyCube : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqManagementHarmonyCube req = await ReadData<ReqManagementHarmonyCube>();
            User user = GetUser();

            ResManagementHarmonyCube response = new();

            ItemData? harmonyCubeItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn);
            if (harmonyCubeItem == null)
            {
                throw new BadHttpRequestException("Harmony cube not found", 404);
            }

            if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(harmonyCubeItem.ItemType, out ItemHarmonyCubeRecord? harmonyCubeData))
            {
                throw new BadHttpRequestException("Item is not a harmony cube", 400);
            }

            ItemHarmonyCubeLevelRecord? currentLevelData = GetCurrentLevelData(harmonyCubeItem, harmonyCubeData);
            int maxSlots = currentLevelData?.Slot ?? 1;

            foreach (long clearCsn in req.Clears)
            {
                if (harmonyCubeItem.CsnList.Contains(clearCsn))
                {
                    harmonyCubeItem.CsnList.Remove(clearCsn);
                }

                if (harmonyCubeItem.Csn == clearCsn)
                {
                    harmonyCubeItem.Csn = 0;
                    harmonyCubeItem.Position = 0;
                }
            }

            foreach (NetWearHarmonyCubeData wearData in req.Wears)
            {
                long targetCsn = wearData.Csn;
                long swapCsn = wearData.SwapCsn;

                if (swapCsn > 0)
                {
                    if (harmonyCubeItem.CsnList.Contains(swapCsn))
                    {
                        harmonyCubeItem.CsnList.Remove(swapCsn);
                    }

                    if (harmonyCubeItem.Csn == swapCsn)
                    {
                        harmonyCubeItem.Csn = 0;
                        harmonyCubeItem.Position = 0;
                    }
                }

                if (targetCsn > 0)
                {
                    EquipHarmonyCubeToCharacter(user, harmonyCubeItem, harmonyCubeData, targetCsn, maxSlots);
                }
            }

            List<ItemData> allHarmonyCubes = user.Items.Where(item =>
                GameData.Instance.ItemHarmonyCubeTable.ContainsKey(item.ItemType)).ToList();

            foreach (ItemData harmonyCube in allHarmonyCubes)
            {
                NetUserHarmonyCubeData netHarmonyCube = new()
                {
                    Isn = harmonyCube.Isn,
                    Tid = harmonyCube.ItemType,
                    Lv = harmonyCube.Level
                };

                foreach (long csn in harmonyCube.CsnList)
                {
                    netHarmonyCube.CsnList.Add(csn);
                }

                if (harmonyCube.Csn > 0 && !harmonyCube.CsnList.Contains(harmonyCube.Csn))
                {
                    netHarmonyCube.CsnList.Add(harmonyCube.Csn);
                }

                response.HarmonyCubes.Add(netHarmonyCube);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private void EquipHarmonyCubeToCharacter(User user, ItemData harmonyCubeItem, ItemHarmonyCubeRecord harmonyCubeData, long targetCsn, int maxSlots)
        {
            if (harmonyCubeItem.CsnList.Contains(targetCsn))
            {
                return; // Already equipped, skip
            }

            if (harmonyCubeItem.CsnList.Count >= maxSlots)
            {
                throw new BadHttpRequestException($"Harmony cube slot limit reached. Current level allows {maxSlots} characters.", 400);
            }

            CharacterModel? character = user.GetCharacterBySerialNumber(targetCsn);
            if (character == null)
            {
                throw new BadHttpRequestException($"Character {targetCsn} not found", 404);
            }

            if (!IsClassCompatible(character, harmonyCubeData))
            {
                throw new BadHttpRequestException($"Character class incompatible with harmony cube", 400);
            }

            CleanupCharacterFromAllHarmonyCubes(user, targetCsn, harmonyCubeData.LocationId, harmonyCubeItem.Isn);

            harmonyCubeItem.CsnList.Add(targetCsn);

            if (harmonyCubeItem.CsnList.Count == 1)
            {
                harmonyCubeItem.Csn = targetCsn;
                harmonyCubeItem.Position = harmonyCubeData.LocationId;
            }

        }

        private void CleanupCharacterFromAllHarmonyCubes(User user, long targetCsn, int position, long excludeIsn)
        {
            foreach (ItemData item in user.Items.ToArray())
            {
                if (!GameData.Instance.ItemHarmonyCubeTable.ContainsKey(item.ItemType) ||
                    item.Isn == excludeIsn)
                {
                    continue;
                }

                if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(item.ItemType, out ItemHarmonyCubeRecord? existingHarmonyCubeData))
                {
                    continue;
                }

                bool wasInCsnList = item.CsnList.Contains(targetCsn);
                bool wasInLegacyCsn = item.Csn == targetCsn;

                if (wasInCsnList || wasInLegacyCsn)
                {
                    item.CsnList.Remove(targetCsn);

                    if (item.CsnList.Count > 0)
                    {
                        item.Csn = item.CsnList[0];
                        item.Position = existingHarmonyCubeData.LocationId;
                    }
                    else
                    {
                        item.Csn = 0;
                        item.Position = 0;
                    }

                }
            }
        }

        private ItemHarmonyCubeLevelRecord? GetCurrentLevelData(ItemData harmonyCubeItem, ItemHarmonyCubeRecord harmonyCubeData)
        {
            List<ItemHarmonyCubeLevelRecord> levelData = GameData.Instance.ItemHarmonyCubeLevelTable.Values
                .Where(x => x.LevelEnhanceId == harmonyCubeData.LevelEnhanceId)
                .OrderBy(x => x.Level)
                .ToList();

            return levelData.FirstOrDefault(x => x.Level == harmonyCubeItem.Level);
        }

        private bool IsClassCompatible(CharacterModel character, ItemHarmonyCubeRecord harmonyCubeData)
        {
            if (GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? characterData))
            {
                return harmonyCubeData.Class == CharacterClassType.All || harmonyCubeData.Class == characterData.Class;
            }
            return false;
        }
    }
}
