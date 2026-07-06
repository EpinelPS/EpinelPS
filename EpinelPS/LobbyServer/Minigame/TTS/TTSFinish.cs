using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Finish")]
public class TTSFinish : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishMiniGameTtsPlay req = await ReadData<ReqFinishMiniGameTtsPlay>();
        User user = GetUser();
        ResFinishMiniGameTtsPlay response = new();

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
           NetMiniGameTtsSongPlayData? predata  = ttsData.SongPlayData
                .Where(x => x.EventTtsSongManagerTableId == req.EventTtsSongManagerTableId && x.Difficulty == req.Difficulty).FirstOrDefault();
            if (predata!=null)
            {
                response.PreviousSongPlayData = predata;
            }
            ttsData.AllPlayCount += 1;
            ttsData.TotalScore += req.Score;
            TtsHelper.InsertOrUpdate(new()
            {
                Difficulty = req.Difficulty,
                IsDeleted = 0,
                RankType = MiniGameTtsRankingType.Server,
                Score = req.Score,
                SongId = req.EventTtsSongManagerTableId,
                UserId = (long)user.ID,
                UpdateTime = DateTime.Now.Ticks
            });

            TtsHelper.InsertOrUpdate(new()
            {
                Difficulty = req.Difficulty,
                IsDeleted = 0,
                RankType = MiniGameTtsRankingType.Friend,
                Score = req.Score,
                SongId = req.EventTtsSongManagerTableId,
                UserId = (long)user.ID,
                UpdateTime = DateTime.Now.Ticks
            });

            TtsHelper.InsertOrUpdate(new()
            {
                Difficulty = req.Difficulty,
                IsDeleted = 0,
                RankType = MiniGameTtsRankingType.Union,
                Score = req.Score,
                SongId = req.EventTtsSongManagerTableId,
                UserId = (long)user.ID,
                UpdateTime = DateTime.Now.Ticks
            });
            RankData rank = JsonDb.GetRank();

            rank.TtsRankDatas.InsertOrUpdate((long)user.ID, MiniGameTtsRankingType.Server, ttsData.TotalScore);
            rank.TtsRankDatas.InsertOrUpdate((long)user.ID, MiniGameTtsRankingType.Friend, ttsData.TotalScore);
            rank.TtsRankDatas.InsertOrUpdate((long)user.ID, MiniGameTtsRankingType.Union, ttsData.TotalScore);

            //ScoreData, PlayCount
            AddScoreData(req, ref ttsData);

            if (req.IsCleared)
            {
                user.AddUnique(ttsData.BadgeSongId, req.EventTtsSongManagerTableId);

                
                Dictionary<int, NetMiniGameTtsBadgeData> badge = new();
                badge.TryAdd(req.EventTtsSongManagerTableId, new() { HasEntered = true, HasNewSongBadge = true, HasReceivableReward = false });
                ttsData.BadgeData.TryAdd(req.Difficulty, badge);
            }

            ttsData.TotalPlayTime = req.TotalPlayTime;

            ttsData.LastDifficulty = req.Difficulty;

            user.AddTrigger(Trigger.EventMiniGameTTSPlayCheck, 1, req.EventTtsSongManagerTableId);

            if (Chkhigh(req.EventTtsSongManagerTableId, req.Difficulty, req.Score, ref ttsData))
            {
                response.IsNewRecord = true;
            }
            else
            { response.IsNewRecord = false; }
            response.PlayCount = ttsData.AllPlayCount;

            UpMission(ref ttsData, req);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }

    private static bool Chkhigh(int songId, MiniGameTtsDifficulty difficulty,int score, ref TtsDatas ttsData)
    {
        NetMiniGameTtsScoreData? existing = ttsData.ScoreData.FirstOrDefault(x =>
        x.EventTtsSongManagerTableId == songId &&
        x.Difficulty == difficulty);
        if (existing != null)
        {
            if (score > existing.Score)
            {
                return true;
            }
           
        }
        return false;
    }

    private static void UpMission(ref TtsDatas ttsData, ReqFinishMiniGameTtsPlay req)
    {
        List<EventTTSMissionType> lable = new();
        lable.Add(EventTTSMissionType.MusicPlayCount);
        lable.Add(EventTTSMissionType.ScoreAccumulate);

        if ((int)req.Rank >= (int)MiniGameTtsRank.A)
        {
            lable.Add(EventTTSMissionType.MusicClearCountByRankA);
        }
        else if ((int)req.Rank >= (int)MiniGameTtsRank.S)
        {
            lable.Add(EventTTSMissionType.MusicClearCountByRankS);
        }
        else if ((int)req.Rank >= (int)MiniGameTtsRank.Splus)
        {
            lable.Add(EventTTSMissionType.MusicClearCountByRankSS);
        }

        if (req.GreatCount > 0 )
        {
            lable.Add(EventTTSMissionType.NoteRankCountByGreat);
        }
        else if (req.PerfectCount > 0)
        {
            lable.Add(EventTTSMissionType.NoteRankCountByGreat);
            lable.Add(EventTTSMissionType.NoteRankCountByPerfect);
        }
        else if (req.PerfectPlusCount > 0)
        {
            lable.Add(EventTTSMissionType.NoteRankCountByGreat);
            lable.Add(EventTTSMissionType.NoteRankCountByPerfect);
            lable.Add(EventTTSMissionType.NoteRankCountByPerfectPlus);
        }

        List<int>? specificMusic = [.. GameData.Instance.EventTTSMissionTable.Values
            .Where(s => s.MissionType == EventTTSMissionType.SpecificMusicPlayCount)
            .SelectMany(s => s.MissionValue)];

        if (specificMusic.Contains(req.EventTtsSongManagerTableId))
        {
            lable.Add(EventTTSMissionType.SpecificMusicPlayCount);
        }
        List<int>? skinlist = GameData.Instance.EventTTSSkinObjectTable.Values
            .Where(x => x.IsFree == false)
            .Select(x=>x.Id)
            .ToList();
        List<int> userlist = new();
        userlist.AddUnique(ttsData.SkinData.FirstCharacterSkinObjectId);
        userlist.AddUnique(ttsData.SkinData.SecondCharacterSkinObjectId);
        userlist.AddUnique(ttsData.SkinData.ThirdCharacterSkinObjectId);
        bool hasAny = skinlist.Any(x => userlist.Contains(x));
        if (hasAny)
        {
            lable.Add(EventTTSMissionType.AnyMusicPlayCountWithSkinObject);
        }
        List<int> matchedItems = skinlist.Intersect(userlist).ToList();

        foreach (var item in lable)
        {
            switch (item)
            {
                case EventTTSMissionType.MusicClearCountByCount:
                    break;
                case EventTTSMissionType.MusicClearCountByRankA:
                    var cra = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s => s.MissionType == EventTTSMissionType.MusicClearCountByRankA)
                        .ToList();

                    foreach (var miss in cra)
                    {
                        if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                        {
                            vale.Progress += 1;
                        }
                    }
                    break;
                case EventTTSMissionType.MusicClearCountByRankS:
                    break;
                case EventTTSMissionType.MusicClearCountByRankSS:
                    break;
                case EventTTSMissionType.MusicClearCountByFullCombo:
                    break;
                case EventTTSMissionType.MusicClearCountByPerfect:
                    break;
                case EventTTSMissionType.NoteRankCountByGood:
                    break;
                case EventTTSMissionType.NoteRankCountByGreat:
                    var nrg = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s => s.MissionType == EventTTSMissionType.NoteRankCountByGreat)
                        .ToList();

                    foreach (var miss in nrg)
                    {
                        if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                        {
                            vale.Progress += (req.GreatCount + req.PerfectCount + req.PerfectPlusCount);
                        }
                    }
                    break;
                case EventTTSMissionType.NoteRankCountByPerfect:
                    break;
                case EventTTSMissionType.NoteRankCountByPerfectPlus:
                    break;
                case EventTTSMissionType.SpecificMusicClearCount:
                    break;
                case EventTTSMissionType.ScoreAccumulate:
                    var sam = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s => s.MissionType == EventTTSMissionType.ScoreAccumulate)
                        .ToList();

                    foreach (var miss in sam)
                    {
                        if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                        {
                            vale.Progress = (int)ttsData.TotalScore;
                        }
                    }
                    break;
                case EventTTSMissionType.MusicPlayCount:
                    var pc = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s => s.MissionType == EventTTSMissionType.MusicPlayCount)
                        .ToList();

                    foreach (var miss in pc)
                    {
                        if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                        {
                            vale.Progress += 1;
                        }
                    }

                    break;
                case EventTTSMissionType.SpecificMusicPlayCount:
                    var missions = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s =>s.MissionType == EventTTSMissionType.SpecificMusicPlayCount &&
                        s.MissionValue.Contains(req.EventTtsSongManagerTableId))
                        .ToList();

                    foreach (var miss in missions)
                    {
                        if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                        {
                            vale.Progress += 1;
                        }
                    }
                    break;
                case EventTTSMissionType.AnyMusicPlayCountWithSkinObject:

                    foreach (var aitem in matchedItems)
                    {
                        List<EventTTSMissionRecord_Raw>? amissions = GameData.Instance.EventTTSMissionTable.Values
                        .Where(s => s.MissionType == EventTTSMissionType.AnyMusicPlayCountWithSkinObject &&
                        s.MissionValue.Contains(aitem))
                        .ToList();

                        foreach (var miss in amissions)
                        {
                            if (ttsData.MissionData.TryGetValue(miss.Id, out var vale))
                            {
                                vale.Progress += 1;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

        
        }
    }

    private static void AddScoreData(ReqFinishMiniGameTtsPlay req,ref TtsDatas ttsData)
    {
        NetMiniGameTtsScoreData? existing = ttsData.ScoreData.FirstOrDefault(x =>
        x.EventTtsSongManagerTableId == req.EventTtsSongManagerTableId &&
        x.Difficulty == req.Difficulty);
        if (existing != null)
        {
            existing.Score = Math.Max(existing.Score, req.Score);
        }
        else
        {
            ttsData.ScoreData.Add(new NetMiniGameTtsScoreData
            {
                Difficulty = req.Difficulty,
                EventTtsSongManagerTableId = req.EventTtsSongManagerTableId,
                Score = req.Score
            });
        }

        NetMiniGameTtsSongPlayCount? playrecord = ttsData.SongPlayCount.FirstOrDefault(p => 
        p.EventTtsSongManagerTableId == req.EventTtsSongManagerTableId &&
        p.Difficulty == req.Difficulty);

        if (playrecord != null)
        {
            playrecord.PlayCount += 1;
        }
        else
        {
            ttsData.SongPlayCount.Add(new NetMiniGameTtsSongPlayCount ()
            {
                Difficulty = req.Difficulty,
                EventTtsSongManagerTableId = req.EventTtsSongManagerTableId,
                PlayCount = 1
            });
        }

        NetMiniGameTtsSongPlayData? playdata = ttsData.SongPlayData.FirstOrDefault(d =>
        d.EventTtsSongManagerTableId == req.EventTtsSongManagerTableId &&
        d.Difficulty == req.Difficulty);

        if (playdata != null)
        {

            playdata.ComboCount = Math.Max(playdata.ComboCount, req.ComboCount);
            playdata.Score = Math.Max(playdata.Score, req.Score);
            if (req.IsCleared)
            {
                playdata.IsCleared = true;
            }
            playdata.GoodCount = Math.Max(playdata.GoodCount, req.GoodCount);
            playdata.GreatCount = Math.Max(playdata.GreatCount, req.GreatCount);
            if (req.IsAllPerfect)
            {
                playdata.IsAllPerfect = true;
            }
            if (req.IsFullCombo)
            {
                playdata.IsFullCombo = true;
            }
            playdata.MissCount = Math.Min(playdata.MissCount, req.MissCount);
            playdata.PerfectCount = Math.Max(playdata.PerfectCount, req.PerfectCount);
            playdata.PerfectPlusCount = Math.Max(playdata.PerfectPlusCount, req.PerfectPlusCount);
            playdata.Rank = (MiniGameTtsRank)Math.Max((int)playdata.Rank, (int)req.Rank);
           

        }
        else
        {
            NetMiniGameTtsSongPlayData newplay = new()
            {
                EventTtsSongManagerTableId = req.EventTtsSongManagerTableId,
                Difficulty = req.Difficulty,
                ComboCount = req.ComboCount,
                Score = req.Score,
                IsCleared = req.IsCleared,
                GoodCount = req.GoodCount,
                GreatCount = req.GreatCount,
                IsAllPerfect = req.IsAllPerfect,
                IsFullCombo = req.IsFullCombo,
                MissCount = req.MissCount,
                PerfectCount = req.PerfectCount,
                PerfectPlusCount = req.PerfectPlusCount,
                Rank = req.Rank
            };

            ttsData.SongPlayData.Add(newplay);
        }
    }
}