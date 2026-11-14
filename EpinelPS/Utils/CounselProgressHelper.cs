using EpinelPS.Data;

namespace EpinelPS.Utils;

public readonly record struct CounselUnlockResult(int UpdatedCharacters, int AddedEntries);

public static class CounselProgressHelper
{
    public static CounselUnlockResult UnlockAll(
        User user,
        IReadOnlyDictionary<int, CharacterRecord> characters,
        IReadOnlyDictionary<int, AttractiveCounselRecord> counsels)
    {
        if (characters.Count == 0 || counsels.Count == 0)
            throw new InvalidOperationException("Character or counsel static data is unavailable. No progress was changed.");

        var ownedNameCodes = user.Characters
            .Where(character => characters.ContainsKey(character.Tid))
            .Select(character => characters[character.Tid].NameCode)
            .ToHashSet();

        int updatedCharacters = 0;
        int addedEntries = 0;
        foreach (var group in counsels.Values
            .Where(counsel => counsel.Id > 0 && ownedNameCodes.Contains(counsel.NameCode))
            .GroupBy(counsel => counsel.NameCode))
        {
            var bond = user.BondInfo.FirstOrDefault(item => item.NameCode == group.Key);
            if (bond == null)
            {
                bond = new NetUserAttractiveData { NameCode = group.Key, Lv = 1 };
                user.BondInfo.Add(bond);
            }

            // Keep legacy IDs and all unrelated bond fields intact.
            var completedIds = bond.CounselDialogCompleteIds.ToHashSet();
            bool changed = false;
            foreach (int id in group.Select(counsel => counsel.Id).Distinct().Order())
            {
                if (!completedIds.Add(id)) continue;
                bond.CounselDialogCompleteIds.Add(id);
                addedEntries++;
                changed = true;
            }

            // Completing a log implies at least one counsel, without reducing prior counts.
            int count = Math.Max(bond.CounseledCount, completedIds.Count);
            if (count != bond.CounseledCount)
            {
                bond.CounseledCount = count;
                changed = true;
            }

            if (changed) updatedCharacters++;
        }

        return new CounselUnlockResult(updatedCharacters, addedEntries);
    }

    public static bool HasCounseledBefore(User user, int counselId) =>
        counselId > 0 && user.BondInfo.Any(bond => bond.CounselDialogCompleteIds.Contains(counselId));
}
