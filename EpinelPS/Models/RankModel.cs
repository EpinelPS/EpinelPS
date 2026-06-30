using EpinelPS.Database;
using System.Reflection.PortableExecutable;

namespace EpinelPS.Models;

public class RankData
{
    public Dictionary<int, List<SongRankData>> SongRankDatas { get; set; } = [];
    public MiniGameTtsTotalRankData TtsRankDatas { get; set; } = new();




   
}


public class MiniGameTtsTotalRankData
{
    public List<MiniGameTtsTotalRankRecord> TtsTotalRankRecords { get; set; } = [];

    /// <summary>
    /// 根据 UserId + RankType 查找记录
    /// </summary>
    public MiniGameTtsTotalRankRecord? FindRecord(long userId, MiniGameTtsRankingType rankType)
    {
        return TtsTotalRankRecords.FirstOrDefault(r => r.UserId == userId && r.RankType == rankType);
    }

    /// <summary>
    /// 插入或更新记录（UPSERT）
    /// </summary>
    public void InsertOrUpdate(long userId, MiniGameTtsRankingType rankType, long score)
    {
        var existing = FindRecord(userId, rankType);

        if (existing != null)
        {
            // 更新现有记录
            existing.Score = score;
            existing.UpdatedAt = DateTime.UtcNow.Ticks;
        }
        else
        {
            // 插入新记录
            TtsTotalRankRecords.Add(new MiniGameTtsTotalRankRecord
            {                
                UserId = userId,
                RankType = rankType,
                Score = score,
                UpdatedAt = DateTime.UtcNow.Ticks
            });
        }
        JsonDb.Save();
    }

    /// <summary>
    /// 获取用户特定类型的排名（按分数降序计算）
    /// </summary>
    public (long Score, int Rank)? GetUserRank(long userId, MiniGameTtsRankingType rankType)
    {
        var ranked = TtsTotalRankRecords
            .Where(r => r.RankType == rankType)
            .OrderByDescending(r => r.Score)
            .ToList();

        var record = ranked.FirstOrDefault(r => r.UserId == userId);
        if (record == null) return null;

        int rank = ranked.IndexOf(record) + 1;
        return (record.Score, rank);
    }

    /// <summary>
    /// 获取指定类型的全部排行榜
    /// </summary>
    /// <param name="rankType">类型0=Union, 1=Friend, 2=Server</param>
    /// <returns></returns>
    public List<NetMiniGameTtsTotalRankData> TotalGetLeaderboardByRankType(MiniGameTtsRankingType rankType)
    {
        
        var result = new List<NetMiniGameTtsTotalRankData>();

        // 获取该类型下所有记录，按分数降序排列
        var ranked = TtsTotalRankRecords
            .Where(x => x.RankType == rankType)
            .OrderByDescending(x => x.Score)
            .ToList();

        if (ranked.Count == 0)
            return result;

        // 构建返回数据
        for (int i = 0; i < ranked.Count; i++)
        {
            var data = new NetMiniGameTtsTotalRankData
            {
                User = CreateWholeUserDataFromDbUser(ranked[i].UserId),
                Score = ranked[i].Score,
                Position = i + 1
            };
            result.Add(data);
        }

        return result;
    }

    /// <summary>
    /// 获取用户在指定类型中的排名
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="rankType">类型0=Union, 1=Friend, 2=Server</param>
    /// <returns></returns>
    public NetMiniGameTtsTotalRankData? TotalGetUserRank(long userId, MiniGameTtsRankingType rankType)
    {      

        // 获取该类型下所有记录，按分数降序排列
        var ranked = TtsTotalRankRecords
            .Where(x => x.RankType == rankType)
            .OrderByDescending(x => x.Score)
            .ToList();

        if (ranked.Count == 0)
            return null;

        // 查找用户记录
        var userRecord = ranked.FirstOrDefault(x => x.UserId == userId);
        if (userRecord == null)
            return null;

        // 计算排名
        int rankPosition = ranked.IndexOf(userRecord) + 1;

        // 构建返回数据
        return new NetMiniGameTtsTotalRankData
        {
            User = CreateWholeUserDataFromDbUser(userRecord.UserId),
            Score = userRecord.Score,
            Position = rankPosition
        };
    }


    /// <summary>
    /// 获取排行榜（指定类型的前N名）
    /// </summary>
    public List<(MiniGameTtsTotalRankRecord Record, int Rank)> TotalGetLeaderboardByRankType(
        MiniGameTtsRankingType rankType,
        int? limit = null)
    {
        var ranked = TtsTotalRankRecords
            .Where(r => r.RankType == rankType)
            .OrderByDescending(r => r.Score)
            .ToList();

        if (limit.HasValue && limit.Value > 0)
        {
            ranked = ranked.Take(limit.Value).ToList();
        }

        var result = new List<(MiniGameTtsTotalRankRecord, int)>();
        for (int i = 0; i < ranked.Count; i++)
        {
            result.Add((ranked[i], i + 1));
        }
        return result;
    }


    /// <summary>
    /// 获取指定类型的排行榜（用于返回 NetMiniGameTtsTotalRankData）
    /// </summary>
    public List<NetMiniGameTtsTotalRankData> GetLeaderboardForResponse(
        MiniGameTtsRankingType rankType,
        int? limit = null)
    {
        var ranked = TtsTotalRankRecords
            .Where(r => r.RankType == rankType)
            .OrderByDescending(r => r.Score)
            .ToList();

        if (limit.HasValue && limit.Value > 0)
        {
            ranked = ranked.Take(limit.Value).ToList();
        }

        var result = new List<NetMiniGameTtsTotalRankData>();
        for (int i = 0; i < ranked.Count; i++)
        {
            var data = new NetMiniGameTtsTotalRankData
            {
                User = CreateWholeUserDataFromDbUser(ranked[i].UserId),
                Score = ranked[i].Score,
                Position = i + 1
            };
            result.Add(data);
        }
        return result;
    }

    /// <summary>
    /// 获取用户在指定类型中的分数
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="rankType">类型0=Union, 1=Friend, 2=Server</param>
    /// <returns></returns>
    public long TotalGetUserScore(long userId, MiniGameTtsRankingType rankType)
    {        

        var record = TtsTotalRankRecords
            .FirstOrDefault(x => x.UserId == userId && x.RankType == rankType);

        return record?.Score ?? 0;
    }


    /// <summary>
    /// 获取排行榜总人数
    /// </summary>
    /// <param name="rankType">类型0=Union, 1=Friend, 2=Server</param>
    /// <returns></returns>
    public int TotalGetTotalCount(MiniGameTtsRankingType rankType)
    {     

        return TtsTotalRankRecords
            .Count(x => x.RankType == rankType);
    }


    /// <summary>
    /// 删除用户在指定类型中的记录
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="rankType">类型0=Union, 1=Friend, 2=Server</param>
    public void TotalDeleteUserRank(long userId, MiniGameTtsRankingType rankType)
    {
        int removed = TtsTotalRankRecords
            .RemoveAll(x => x.UserId == userId && x.RankType == rankType);

        if (removed > 0)
        {
            JsonDb.Save();
        }
    }



    /// <summary>
    /// 删除指定用户的所有记录
    /// </summary>
    public int DeleteAllByUser(long userId)
    {
        return TtsTotalRankRecords.RemoveAll(r => r.UserId == userId);
    }

    /// <summary>
    /// 检查是否存在指定记录
    /// </summary>
    public bool Exists(long userId, MiniGameTtsRankingType rankType)
    {
        return FindRecord(userId, rankType) != null;
    }

    /// <summary>
    /// 获取用户在指定类型中的分数（不存在时返回 0）
    /// </summary>
    public long GetUserScore(long userId, MiniGameTtsRankingType rankType)
    {
        return FindRecord(userId, rankType)?.Score ?? 0;
    }

    /// <summary>
    /// 获取指定类型的记录总数
    /// </summary>
    public int GetRecordCount(MiniGameTtsRankingType rankType)
    {
        return TtsTotalRankRecords.Count(r => r.RankType == rankType);
    }

    /// <summary>
    /// 通过id创建用户数据
    /// </summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    public static NetWholeUserData CreateWholeUserDataFromDbUser(long userid)
    {

        User? user = JsonDb.GetUser((ulong)userid);
        if (user != null)
        {


            NetWholeUserData ret = new()
            {
                Lv = user.userPointData.UserLevel,
                Frame = user.ProfileFrame,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
                UserTitleId = user.TitleId,
                Nickname = user.Nickname,
                Usn = (long)user.ID,
                LastActionAt = DateTimeOffset.UtcNow.Ticks,
                Server = 1001
            };

            return ret;
        }
        else
        {
            return null;
        }
    }
}