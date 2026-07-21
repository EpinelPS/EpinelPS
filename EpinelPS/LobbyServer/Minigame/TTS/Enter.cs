using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Shop;
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
            .Where(x => x.BasicMusicGroup == true).ToList();

        EventTTSManagerRecord_Raw? tutorial = GameData.Instance.EventTTSManagerTable.Values
            .Where(t => t.Id == req.EventTtsManagerTableId).FirstOrDefault();

        List<int>? Unlocksong = GameData.Instance.EventTTSSongManagerTable.Values
            .Select(x => x.Id)
            .ToList();

        if (!user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            ttsData = new TtsDatas();
            ttsData.UnlockSongId.AddRange(Unlocksong);
            ttsData.PurchasedAlbumIds.AddRange([1001, 1002]);
            IntMission(ref ttsData);
            user.TTSGameData[req.EventTtsManagerTableId] = ttsData;
        }

        response.HasFinishedTutorial = ttsData.IsFinishTutorial;
        response.LastPlayedDifficulty = ttsData.LastDifficulty;
        var missionList = MiniGameHelper.ToProtoDict<int, NetMiniGameTtsMissionData, MiniGameTtsMissionData>(ttsData.MissionData);
        response.MissionDataList.AddRange(missionList.Values);
        //response.NewBadgeEventTtsSongManagerTableIds.AddRange(ttsData.BadgeSongId);
        //response.NewUnlockedEventTtsSongManagerTableIds.AddRange(ttsData.UnlockSongId);
        response.PlayCount = ttsData.AllPlayCount;
        response.PurchasedAlbumIds.AddRange(ttsData.PurchasedAlbumIds);
        response.ServerTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow);
        var songPlayList = MiniGameHelper.ToProtoList<NetMiniGameTtsSongPlayData, MiniGameTtsSongPlayData>(ttsData.SongPlayData);
        response.SongPlayDataList.AddRange(songPlayList);
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
        if (ttsData.SkinData.BackgroundSkinObjectId == 0)
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
            ttsData.SkinData = MiniGameHelper.FromProto<UserMiniGameTtsSkinData, NetUserMiniGameTtsSkinData>(ttsSkinData);
            response.UserSkinData = ttsSkinData;
            foreach (var item in skin)
            {
                ttsData.BuySkinObject.Add(item.Id);
            }
        }
        else
        {
            response.UserSkinData = MiniGameHelper.ToProto<NetUserMiniGameTtsSkinData, UserMiniGameTtsSkinData>(ttsData.SkinData);
        }

        if (ttsData.PurchasedAlbumIds.Count > 0)
        {
            response.ExpertUnlockResult = MiniGameTtsExpertUnlockResult.AlreadyUnlocked;
        }
        else
        {
            response.ExpertUnlockResult = MiniGameTtsExpertUnlockResult.NotUnlocked;
        }

        RankData rank = GetRank();

        NetMiniGameTtsTotalRankData? myfrank = rank.TtsRankDatas.TotalGetUserRank((long)user.ID, MiniGameTtsRankingType.Friend);
        if (myfrank != null)
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

        if (ttsData.UnlockSongId.Count < Unlocksong.Count)
        {
            ttsData.PurchasedAlbumIds.AddRangeUnique([1001, 1002]);
            ttsData.UnlockSongId.AddRangeUnique(Unlocksong);
        }

        response.UserData = LobbyHandler.CreateWholeUserDataFromDbUser(user);

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
