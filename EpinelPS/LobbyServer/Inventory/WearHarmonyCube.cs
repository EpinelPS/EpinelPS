using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearharmonycube")]
    public class WearHarmonyCube : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqWearHarmonyCube req = await ReadData<ReqWearHarmonyCube>();
            User user = GetUser();
            ResWearHarmonyCube response = new();

            ItemData? harmonyCubeItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn);
            if (harmonyCubeItem == null)
            {
                throw new BadHttpRequestException("Harmony cube not found", 404);
            }

            if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(harmonyCubeItem.ItemType, out ItemHarmonyCubeRecord? harmonyCubeData))
            {
                throw new BadHttpRequestException("Item is not a harmony cube", 400);
            }

            if (req.Wear == null)
            {
                throw new BadHttpRequestException("Wear data is required", 400);
            }

            long targetCsn = req.Wear.Csn;
            long swapCsn = req.Wear.SwapCsn;

            ItemHarmonyCubeLevelRecord? currentLevelData = GetCurrentLevelData(harmonyCubeItem, harmonyCubeData);
            int maxSlots = currentLevelData?.Slot ?? 1;

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

            List<ItemData> modifiedItems = new();
            if (targetCsn > 0)
            {
                modifiedItems = EquipHarmonyCubeToCharacter(user, harmonyCubeItem, harmonyCubeData, targetCsn, maxSlots);
            }
            else if (targetCsn == 0)
            {
                harmonyCubeItem.CsnList.Clear();
                harmonyCubeItem.Csn = 0;
                harmonyCubeItem.Position = 0;
            }
            else
            {
                throw new BadHttpRequestException("InvalId character CSN", 400);
            }

            foreach (ItemData modifiedItem in modifiedItems)
            {
                NetUserHarmonyCubeData netModifiedHarmonyCube = new()
                {
                    Isn = modifiedItem.Isn,
                    Tid = modifiedItem.ItemType,
                    Lv = modifiedItem.Level
                };

                foreach (long csn in modifiedItem.CsnList)
                {
                    netModifiedHarmonyCube.CsnList.Add(csn);
                }

                if (modifiedItem.Csn > 0 && !modifiedItem.CsnList.Contains(modifiedItem.Csn))
                {
                    netModifiedHarmonyCube.CsnList.Add(modifiedItem.Csn);
                }

                response.HarmonyCubes.Add(netModifiedHarmonyCube);
            }

            NetUserHarmonyCubeData netHarmonyCube = new()
            {
                Isn = harmonyCubeItem.Isn,
                Tid = harmonyCubeItem.ItemType,
                Lv = harmonyCubeItem.Level
            };

            foreach (long csn in harmonyCubeItem.CsnList)
            {
                netHarmonyCube.CsnList.Add(csn);
            }

            if (harmonyCubeItem.Csn > 0 && !harmonyCubeItem.CsnList.Contains(harmonyCubeItem.Csn))
            {
                netHarmonyCube.CsnList.Add(harmonyCubeItem.Csn);
            }

            response.HarmonyCubes.Add(netHarmonyCube);

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private List<ItemData> EquipHarmonyCubeToCharacter(User user, ItemData harmonyCubeItem, ItemHarmonyCubeRecord harmonyCubeData, long targetCsn, int maxSlots)
        {

            // Check if already equipped to this character
            if (harmonyCubeItem.CsnList.Contains(targetCsn))
            {
                Console.WriteLine($"Harmony cube {harmonyCubeItem.ItemType} already equipped to character {targetCsn}");
                return new List<ItemData>(); // Already equipped, no need to do anything
            }

            // Check slot limit
            if (harmonyCubeItem.CsnList.Count >= maxSlots)
            {
                throw new BadHttpRequestException($"Harmony cube slot limit reached. Current level allows {maxSlots} characters.", 400);
            }

            // Check if the character exists
            CharacterModel? character = user.GetCharacterBySerialNumber(targetCsn);
            if (character == null)
            {
                throw new BadHttpRequestException($"Character {targetCsn} not found", 404);
            }

            // Check class compatibility
            if (!IsClassCompatible(character, harmonyCubeData))
            {
                throw new BadHttpRequestException($"Character class incompatible with harmony cube", 400);
            }

            // CRITICAL: Remove this character from ALL harmony cubes at the same position
            // This fixes any existing data inconsistency where a character might be in multiple CsnLists
            List<ItemData> modifiedItems = CleanupCharacterFromAllHarmonyCubes(user, targetCsn, harmonyCubeData.LocationId, harmonyCubeItem.Isn);

            // Add to CsnList
            harmonyCubeItem.CsnList.Add(targetCsn);

            // For backward compatibility, also set legacy fields if this is the first character
            if (harmonyCubeItem.CsnList.Count == 1)
            {
                harmonyCubeItem.Csn = targetCsn;
                harmonyCubeItem.Position = harmonyCubeData.LocationId;
            }

            Console.WriteLine($"Equipped harmony cube {harmonyCubeItem.ItemType} to character {targetCsn} for user {user.Username} (slot {harmonyCubeItem.CsnList.Count}/{maxSlots})");

            return modifiedItems;
        }

        private List<ItemData> CleanupCharacterFromAllHarmonyCubes(User user, long targetCsn, int position, long excludeIsn)
        {
            // Remove this character from ALL harmony cubes (all positions)
            // This ensures one character can only have one harmony cube equipped at any time
            List<ItemData> modifiedItems = new();

            foreach (ItemData item in user.Items.ToArray())
            {
                // Skip if it's not a harmony cube or it's the item we're about to equip
                if (!GameData.Instance.ItemHarmonyCubeTable.ContainsKey(item.ItemType) ||
                    item.Isn == excludeIsn)
                {
                    continue;
                }

                // Get the harmony cube data
                if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(item.ItemType, out ItemHarmonyCubeRecord? existingHarmonyCubeData))
                {
                    continue;
                }

                // Check ALL harmony cubes (not just same position) - ONE CHARACTER, ONE HARMONY CUBE RULE
                // Check if this character is in the CsnList or legacy Csn field
                bool wasInCsnList = item.CsnList.Contains(targetCsn);
                bool wasInLegacyCsn = (item.Csn == targetCsn);

                if (wasInCsnList || wasInLegacyCsn)
                {
                    // Remove from CsnList
                    item.CsnList.Remove(targetCsn);

                    // Update legacy fields
                    if (item.CsnList.Count > 0)
                    {
                        // Set legacy fields to the first remaining character
                        item.Csn = item.CsnList[0];
                        item.Position = existingHarmonyCubeData.LocationId;
                    }
                    else
                    {
                        // No characters left, clear legacy fields
                        item.Csn = 0;
                        item.Position = 0;
                    }

                    // Add to modified items list for response
                    modifiedItems.Add(item);

                    Console.WriteLine($"[ONE HARMONY CUBE RULE] Removed character {targetCsn} from harmony cube {item.ItemType} (position {existingHarmonyCubeData.LocationId}) - one character can only have one harmony cube");
                }
            }

            return modifiedItems;
        }

        private ItemHarmonyCubeLevelRecord? GetCurrentLevelData(ItemData harmonyCubeItem, ItemHarmonyCubeRecord harmonyCubeData)
        {
            // Get level data for this harmony cube
            List<ItemHarmonyCubeLevelRecord> levelData = GameData.Instance.ItemHarmonyCubeLevelTable.Values
                .Where(x => x.LevelEnhanceId == harmonyCubeData.LevelEnhanceId)
                .OrderBy(x => x.Level)
                .ToList();

            // Find current level data
            return levelData.FirstOrDefault(x => x.Level == harmonyCubeItem.Level);
        }

        private bool IsClassCompatible(CharacterModel character, ItemHarmonyCubeRecord harmonyCubeData)
        {
            // Get character data to check class
            if (GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? characterData))
            {
                // Check if harmony cube class restriction matches character class
                return harmonyCubeData.Class == CharacterClassType.All || harmonyCubeData.Class == characterData.Class;
            }
            return false;
        }

    }
}
