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
        GameUser userNew = GetUserNew();
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
            response.HasSelectProceedOnExpertAlertPopUp = true;
            EventTTSProductNoticeManagerRecord_Raw? noticeid = GameData.Instance.EventTTSProductNoticeManagerTable.Values
                .Where(x => x.NoticeDate.AddDays(1) > DateTime.Now).FirstOrDefault();
            if (noticeid != null)
            {
                response.NewEventTtsProductNoticeManagerTableId = noticeid.Id;
            }            
            if (ttsData.SkinData.BackgroundSkinObjectId ==0)
            {
                List<EventTTSSkinObjectRecord_Raw>? skin = GameData.Instance.EventTTSSkinObjectTable.Values
                    .Where(x => x.IsFree == true).ToList();
                NetUserMiniGameTtsSkinData ttsSkinData = new();
                ttsSkinData.BackgroundSkinObjectId = skin.FirstOrDefault(x => x.SkinObjectType == EventTTSSkinObjectType.BG).Id;
                ttsSkinData.NoteSkinObjectId = skin.FirstOrDefault(x => x.SkinObjectType == EventTTSSkinObjectType.Note).Id;
                var charskin = skin.Where(x => x.SkinObjectType == EventTTSSkinObjectType.Character).ToList();
                if (charskin.Count == 3)
                {
                    ttsSkinData.FirstCharacterSkinObjectId = charskin[0].Id;
                    ttsSkinData.SecondCharacterSkinObjectId = charskin[1].Id;
                    ttsSkinData.ThirdCharacterSkinObjectId = charskin[2].Id;
                }
                ttsData.SkinData = ttsSkinData;
                response.UserSkinData = ttsData.SkinData;
                foreach (var item in skin)
                {
                    ttsData.BuySkinObject.Add(item.Id);
                }
            }
            else
            {
                response.UserSkinData = ttsData.SkinData;
            }

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
            newttsdata.PurchasedAlbumIds.AddRange([1001, 1002]);
            IntMission(ref newttsdata);

            user.TTSGameData.TryAdd(req.EventTtsManagerTableId,newttsdata);
            List<EventTTSSkinObjectRecord_Raw>? skin = GameData.Instance.EventTTSSkinObjectTable.Values
                    .Where(x => x.IsFree == true).ToList();
            NetUserMiniGameTtsSkinData ttsSkinData = new();
            ttsSkinData.BackgroundSkinObjectId = skin.FirstOrDefault(x => x.SkinObjectType == EventTTSSkinObjectType.BG).Id;
            ttsSkinData.NoteSkinObjectId = skin.FirstOrDefault(x => x.SkinObjectType == EventTTSSkinObjectType.Note).Id;
            var charskin = skin.Where(x => x.SkinObjectType == EventTTSSkinObjectType.Character).ToList();
            if (charskin.Count == 3)
            {
                ttsSkinData.FirstCharacterSkinObjectId = charskin[0].Id;
                ttsSkinData.SecondCharacterSkinObjectId = charskin[1].Id;
                ttsSkinData.ThirdCharacterSkinObjectId = charskin[2].Id;
            }
            user.TTSGameData[req.EventTtsManagerTableId].SkinData = ttsSkinData;
            foreach (var item in skin)
            {
                user.TTSGameData[req.EventTtsManagerTableId].BuySkinObject.Add( item.Id);
            }
            response.MissionDataList.AddRange(newttsdata.MissionData.Values.ToList());
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
            response.HasSelectProceedOnExpertAlertPopUp = false;
            EventTTSProductNoticeManagerRecord_Raw? noticeid = GameData.Instance.EventTTSProductNoticeManagerTable.Values
                .Where(x => x.NoticeDate < DateTime.Now).FirstOrDefault();
            if (noticeid != null)
            {
                response.NewEventTtsProductNoticeManagerTableId = noticeid.Id;
            }
        }

        response.UserData = LobbyHandler.CreateWholeUserDataFromDbUser(userNew);

        JsonDb.Save();     

        // TODO
        await WriteDataAsync(response);
    }

    public static void IntMission(ref TtsDatas ttsDatas)
    {
        if (ttsDatas.MissionData.Count > 0)
        {
            var mlist = GameData.Instance.EventTTSMissionTable.Values.ToList();
            if (mlist.Count > ttsDatas.MissionData.Count)
            {
                foreach (var item in mlist)
                {
                    if (!ttsDatas.MissionData.ContainsKey(item.Id))
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
        else
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
