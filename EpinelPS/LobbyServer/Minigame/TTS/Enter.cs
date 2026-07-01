using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Enter")]
public class TTSEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterMiniGameTtsTitle req = await ReadData<ReqEnterMiniGameTtsTitle>();
        User user = GetUser();
        ResEnterMiniGameTtsTitle response = new();

        List<EventTTSSongGroupManagerRecord_Raw>? songlist = GameData.Instance.EventTTSSongGroupManagerTable.Values
            .Where(x=>x.BasicMusicGroup == true).ToList();

        EventTTSManagerRecord_Raw? tutorial = GameData.Instance.EventTTSManagerTable.Values
            .Where(t => t.Id == req.EventTtsManagerTableId).FirstOrDefault();

        List<int> Unlocksong = [];


        foreach (var item in songlist)
        {
            var idList = GameData.Instance.EventTTSSongManagerTable.Values
                .Where(x => x.GroupId == item.Id)
                .Select(x => x.Id) 
                .ToList();
            Unlocksong.AddRange(idList);
        }

        Unlocksong.Add(tutorial.TutorialMusicId);


        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId,out var ttsData))
        {
            response.HasFinishedTutorial = ttsData.IsFinishTutorial;
            response.LastPlayedDifficulty = ttsData.LastDifficulty;
            response.MissionDataList.AddRange(ttsData.MissionData.Values.ToList());            
            //response.NewBadgeEventTtsSongManagerTableIds.AddRange(ttsData.BadgeSongId);
            //response.NewUnlockedEventTtsSongManagerTableIds.AddRange(ttsData.UnlockSongId);
            response.PlayCount = ttsData.AllPlayCount;
            response.PurchasedAlbumIds.AddRange(ttsData.PurchasedAlbumIds);
            response.ServerTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow);
            response.SongPlayDataList.AddRange(ttsData.SongPlayData);
            response.TotalPlayTime = ttsData.TotalPlayTime;
            response.AvailableEventTtsSongManagerTableIds.AddRange(ttsData.UnlockSongId);
            response.AlbumRedDotCutoffDateFromNewProductPopUp = ttsData.NewProductPopUp;
            response.AlbumRedDotCutoffDateFromShop = ttsData.DateFromShop;

            if (ttsData.PurchasedAlbumIds.Count>0)
            {
                response.ExpertUnlockResult = MiniGameTtsExpertUnlockResult.AlreadyUnlocked;
            }
            else
            {
                response.ExpertUnlockResult = MiniGameTtsExpertUnlockResult.NotUnlocked;
            }

            RankData rank = GetRank();

            NetMiniGameTtsTotalRankData? myfrank = rank.TtsRankDatas.TotalGetUserRank((long)user.ID,MiniGameTtsRankingType.Friend);
            if (myfrank!= null )
            {
                NetMyMiniGameTtsTotalRankData mfrank = new()
                {
                    Position = myfrank.Position,
                    Score = myfrank.Score
                };

                response.MyFriendTotalRankData = mfrank;
            }
            else
            {
                response.MyFriendTotalRankData = new()
                {
                    Position = 999,
                    Score = 0                    
                };
            }

            NetMiniGameTtsTotalRankData? myurank = rank.TtsRankDatas.TotalGetUserRank((long)user.ID, MiniGameTtsRankingType.Union);
            if (myurank != null)
            {
                NetMyMiniGameTtsTotalRankData murank = new()
                {
                    Position = myurank.Position,
                    Score = myurank.Score
                };

                response.MyUnionTotalRankData = murank;
            }
            else
            {
                response.MyUnionTotalRankData = new() 
                {
                    Position = 999,
                    Score = 0
                };
            }

            IntMission(ref ttsData);

        }
        else 
        {
            TtsDatas newttsdata = new();            
            newttsdata.UnlockSongId.AddRange(Unlocksong);

            user.TTSGameData.TryAdd(req.EventTtsManagerTableId,newttsdata);
            response.HasFinishedTutorial = false;
            response.LastPlayedDifficulty = MiniGameTtsDifficulty.Normal;
            response.ServerTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-5));
            response.PlayCount = 0;
            response.NewUnlockedEventTtsSongManagerTableIds.AddRange(newttsdata.UnlockSongId);
            response.AvailableEventTtsSongManagerTableIds.AddRange(newttsdata.UnlockSongId);
            response.AlbumRedDotCutoffDateFromNewProductPopUp = Timestamp.FromDateTime(DateTime.UtcNow);
            response.AlbumRedDotCutoffDateFromShop = Timestamp.FromDateTime(DateTime.UtcNow);
            response.ExpertUnlockResult = MiniGameTtsExpertUnlockResult.NotUnlocked;
            response.MyFriendTotalRankData = new();
            response.MyUnionTotalRankData = new();
        }

        response.UserData = LobbyHandler.CreateWholeUserDataFromDbUser(user);

        JsonDb.Save();     

        // TODO
        await WriteDataAsync(response);
    }

    public static void IntMission(ref TtsDatas ttsDatas)
    {
        if (ttsDatas.MissionData.Count == 0)
        {
            var mlist = GameData.Instance.EventTTSMissionTable.Values.ToList();
            foreach (var item in mlist)
            {
                ttsDatas.MissionData.TryAdd(item.Id, new()
                {
                    IsReceived = false,
                    MissionId = item.Id,
                    Progress = 0
                });
            }
        }
    }
}
