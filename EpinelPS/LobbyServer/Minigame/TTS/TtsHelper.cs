using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;
using System.Reflection.PortableExecutable;


namespace EpinelPS.LobbyServer.Minigame.TTS;

public static class TtsHelper
{
   
    public static Duration Add(this Duration left, Duration right)
    {
        return Duration.FromTimeSpan(left.ToTimeSpan()+ right.ToTimeSpan());
    }

    public static Duration Subtract(this Duration left, Duration right)
    {
        return Duration.FromTimeSpan(left.ToTimeSpan() - right.ToTimeSpan());
    }

    // 插入或更新（UPSERT）
    public static void InsertOrUpdate(SongRankData entity)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(entity.SongId, out var list))
        {
            // 如果该 SongId 不存在，创建新列表
            list = [];
            rank.SongRankDatas[entity.SongId] = list;
        }

        // 查找是否已存在相同 UserId + RankType + Difficulty 的记录
        var existing = list.FirstOrDefault(x =>
            x.UserId == entity.UserId &&
            x.RankType == entity.RankType &&
            x.Difficulty == entity.Difficulty);

        if (existing != null)
        {
            // 存在则更新（保留 Id）
            existing.Score = entity.Score;
            existing.UpdateTime = entity.UpdateTime;
            existing.IsDeleted = entity.IsDeleted;
        }
        else
        {
            // 不存在则添加
            list.Add(entity);
        }

        JsonDb.Save();
    }

    // 查询单条记录
    public static SongRankData? Get(SqlSongRankKey key)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(key.SongId, out var list))
        {
            return null;
        }

        return list.FirstOrDefault(x =>
            x.UserId == key.UserId &&
            x.RankType == key.RankType &&
            x.Difficulty == key.Difficulty);
    }

    // 获取排行榜（带动态排名）
    /// <summary>
    /// 获取排行榜（带动态排名）
    /// </summary>
    /// <param name="songId"></param>
    /// <param name="rankType"></param>
    /// <param name="difficulty"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public static List<(SongRankData Entity, int Rank)> GetLeaderboardWithRank(
    int songId,
    MiniGameTtsRankingType rankType,
    MiniGameTtsDifficulty difficulty,
    int limit = 100)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(songId, out var list) || list.Count == 0)
            return [];

        var filtered = list
            .Where(x => x.RankType == rankType &&
                        x.Difficulty == difficulty &&
                        x.IsDeleted == 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        if (limit > 0 && filtered.Count > limit)
            filtered = filtered.Take(limit).ToList();

        var result = new List<(SongRankData Entity, int Rank)>();
        for (int i = 0; i < filtered.Count; i++)
        {
            result.Add((filtered[i], i + 1));
        }

        return result;
    }

    // 获取某个歌曲下所有未删除的排名数据
    public static List<SongRankData> GetBySongId(int songId)
    {
        RankData rank = JsonDb.GetRank();

        if (rank.SongRankDatas.TryGetValue(songId, out var songRanks))
        {
            return songRanks.Where(x => x.IsDeleted == 0).ToList();
        }

        return [];
    }

    // 查询所有未删除的数据
    public static List<SongRankData> GetAll()
    {
        RankData rank = JsonDb.GetRank();
        List<SongRankData> list = [];

        foreach (var kvp in rank.SongRankDatas)
        {
            list.AddRange(kvp.Value.Where(x => x.IsDeleted == 0));
        }

        return list;
    }

    // 获取某个歌曲、某个排名类型、某个难度的排行榜（所有用户的分数排名）
    public static List<SongRankData> GetLeaderboard(int songId, MiniGameTtsRankingType rankType, MiniGameTtsDifficulty difficulty, int limit = 100)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return [];

        var result = songRanks
            .Where(x => x.RankType == rankType &&
                        x.Difficulty == difficulty &&
                        x.IsDeleted == 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        if (limit > 0 && result.Count > limit)
            result = result.Take(limit).ToList();

        return result;
    }




    /// <summary>
    /// 获取个人在某个歌曲、某个排名类型下的所有难度数据（带排名）
    /// </summary>
    public static List<(SongRankData Entity, int Rank)> GetUserSongRankingsWithRank(long userId, int songId, MiniGameTtsRankingType rankType)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return [];

        // 获取该用户在该歌曲、该排名类型下的所有难度记录
        var userRecords = songRanks
            .Where(x => x.UserId == userId &&
                        x.RankType == rankType &&
                        x.IsDeleted == 0)
            .ToList();

        if (userRecords.Count == 0)
            return [];

        var result = new List<(SongRankData Entity, int Rank)>();

        foreach (var userRecord in userRecords)
        {
            // 计算该用户在该难度下的排名
            var sorted = songRanks
                .Where(x => x.RankType == rankType &&
                            x.Difficulty == userRecord.Difficulty &&
                            x.IsDeleted == 0)
                .OrderByDescending(x => x.Score)
                .ToList();

            int rankPosition = sorted.IndexOf(userRecord) + 1;
            result.Add((userRecord, rankPosition));
        }

        // 按难度排序返回
        return result.OrderBy(x => x.Entity.Difficulty).ToList();
    }

    /// <summary>
    /// 获取单个用户的单条记录（带排名）
    /// </summary>
    public static (SongRankData? Entity, int Rank)? GetUserRank(
        long userId,
        int songId,
        MiniGameTtsRankingType rankType,
        MiniGameTtsDifficulty difficulty)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return null;

        // 获取该用户在指定歌曲、排名类型、难度下的记录
        var userRecord = songRanks.FirstOrDefault(x =>
            x.UserId == userId &&
            x.RankType == rankType &&
            x.Difficulty == difficulty &&
            x.IsDeleted == 0);

        if (userRecord == null)
            return null;

        // 计算排名
        var sorted = songRanks
            .Where(x => x.RankType == rankType &&
                        x.Difficulty == difficulty &&
                        x.IsDeleted == 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        int rankPosition = sorted.IndexOf(userRecord) + 1;

        return (userRecord, rankPosition);
    }

    // 软删除
    public static void SoftDelete(SqlSongRankKey key)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(key.SongId, out var songRanks) || songRanks.Count == 0)
            return;

        var record = songRanks.FirstOrDefault(x =>
            x.UserId == key.UserId &&
            x.RankType == key.RankType &&
            x.Difficulty == key.Difficulty);

        if (record != null)
        {
            record.IsDeleted = 1;
            record.UpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            JsonDb.Save();
        }
    }

    // 软删除（批量删除某个歌曲、排名类型、难度下的所有记录）
    public static void SoftDelete(int songId, MiniGameTtsRankingType rankType, MiniGameTtsDifficulty difficulty)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return;

        var records = songRanks
            .Where(x => x.RankType == rankType && x.Difficulty == difficulty)
            .ToList();

        if (records.Count == 0)
            return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var record in records)
        {
            record.IsDeleted = 1;
            record.UpdateTime = now;
        }

        JsonDb.Save();
    }

    // 软删除指定用户的特定记录
    public static void SoftDeleteUserRecord(SqlSongRankKey key)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(key.SongId, out var songRanks) || songRanks.Count == 0)
            return;

        var record = songRanks.FirstOrDefault(x =>
            x.UserId == key.UserId &&
            x.RankType == key.RankType &&
            x.Difficulty == key.Difficulty);

        if (record != null)
        {
            record.IsDeleted = 1;
            record.UpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            JsonDb.Save();
        }
    }
    // 硬删除（物理删除）
    public static void HardDelete(SqlSongRankKey key)
    {
        RankData rank = JsonDb.GetRank();

        if (!rank.SongRankDatas.TryGetValue(key.SongId, out var songRanks) || songRanks.Count == 0)
            return;

        int removed = songRanks.RemoveAll(x =>
            x.UserId == key.UserId &&
            x.RankType == key.RankType &&
            x.Difficulty == key.Difficulty);

        if (removed > 0)
        {
            // 如果该 SongId 下没有记录了，可以选择删除整个 Key（可选）
            // if (songRanks.Count == 0)
            // {
            //     rank.SongRankDatas.Remove(key.SongId);
            // }

            JsonDb.Save();
        }
    }



    /// <summary>
    /// 获取某个歌曲、某个排名类型下的所有难度数据（每个难度单独排名）
    /// </summary>
    public static List<(SongRankData Entity, int Rank)> GetBySongIdAndRankTypeWithRank(
        int songId,
        MiniGameTtsRankingType rankType)
    {
        RankData rank = JsonDb.GetRank();
        var result = new List<(SongRankData Entity, int Rank)>();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return result;

        // 获取该歌曲、该排名类型下的所有难度
        var difficulties = songRanks
            .Where(x => x.RankType == rankType && x.IsDeleted == 0)
            .Select(x => x.Difficulty)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        foreach (var difficulty in difficulties)
        {
            // 获取该难度下的所有记录，按分数降序排列
            var sorted = songRanks
                .Where(x => x.RankType == rankType &&
                            x.Difficulty == difficulty &&
                            x.IsDeleted == 0)
                .OrderByDescending(x => x.Score)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                result.Add((sorted[i], i + 1));
            }
        }

        return result;
    }


    /// <summary>
    /// 获取某个歌曲、某个排名类型下的所有难度数据（按难度分组，每个难度单独排名）
    /// </summary>
    public static Dictionary<MiniGameTtsDifficulty, List<(SongRankData Entity, int Rank)>>
        GetBySongIdAndRankTypeGrouped(int songId, MiniGameTtsRankingType rankType)
    {
        RankData rank = JsonDb.GetRank();
        var result = new Dictionary<MiniGameTtsDifficulty, List<(SongRankData Entity, int Rank)>>();

        if (!rank.SongRankDatas.TryGetValue(songId, out var songRanks) || songRanks.Count == 0)
            return result;

        // 获取该歌曲、该排名类型下的所有难度
        var difficulties = songRanks
            .Where(x => x.RankType == rankType && x.IsDeleted == 0)
            .Select(x => x.Difficulty)
            .Distinct()
            .ToList();

        foreach (var difficulty in difficulties)
        {
            // 获取该难度下的所有记录，按分数降序排列
            var sorted = songRanks
                .Where(x => x.RankType == rankType &&
                            x.Difficulty == difficulty &&
                            x.IsDeleted == 0)
                .OrderByDescending(x => x.Score)
                .ToList();

            var list = new List<(SongRankData Entity, int Rank)>();
            for (int i = 0; i < sorted.Count; i++)
            {
                list.Add((sorted[i], i + 1));
            }

            result[difficulty] = list;
        }

        return result;
    }
}
