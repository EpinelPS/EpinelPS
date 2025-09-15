using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetProfile")]
    public class GetUserProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetProfileData req = await ReadData<ReqGetProfileData>();
            User callingUser = GetUser();
            User? user = GetUser((ulong)req.TargetUsn);
            ResGetProfileData response = new();

            if (user != null)
            {
                response.Data = new NetProfileData
                {
                    User = LobbyHandler.CreateWholeUserDataFromDbUser(user),
                    LastActionAt = DateTimeOffset.UtcNow.Ticks,
                };
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = user.Characters.Count });

                List<CharacterRecord> allCharacters = [.. GameData.Instance.CharacterTable.Values
                .GroupBy(c => c.name_code)  // Group by name_code to treat same name_code as one character                     3999 = marian
                .SelectMany(g => g.Where(c => c.grade_core_id == 1 || c.grade_core_id == 101 || c.grade_core_id == 201 || c.name_code == 3999))];
                List<CharacterRecord> characters = [];

                foreach (CharacterRecord ac in allCharacters)
                {
                    if (user.HasCharacter(ac.id))
                    {
                        characters.Add(ac);
                    }
                }

                // 未找到Corp的table数据先使用自定义值
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = characters.Where(c => c.corporation.Equals("ELYSION")).ToList().Count, CorporationType = 1 });
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = characters.Where(c => c.corporation.Equals("MISSILIS")).ToList().Count, CorporationType = 2 });
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = characters.Where(c => c.corporation.Equals("TETRA")).ToList().Count, CorporationType = 3 });
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = characters.Where(c => c.corporation.Equals("PILGRIM")).ToList().Count, CorporationType = 4 });
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = characters.Where(c => c.corporation.Equals("ABNORMAL")).ToList().Count, CorporationType = 7 });

                response.Data.InfraCoreLv = user.InfraCoreLvl;
                response.Data.LastCampaignNormalStageId = user.LastNormalStageCleared;
                response.Data.LastCampaignHardStageId = user.LastHardStageCleared;
                response.Data.OutpostOpenState = user.MainQuestData.ContainsKey(25);
                response.Data.Exp = user.userPointData.ExperiencePoint;
                response.Data.SynchroLv = user.SynchroDeviceLevel;
                response.Data.SynchroSlotCount = user.SynchroSlots.Count(x => x.CharacterSerialNumber > 0);

                foreach (var rrrp in user.ResearchProgress)
                {
                    response.Data.Recycle.Add(new NetUserRecycleRoomData()
                    {
                        Tid = rrrp.Key,
                        Lv = rrrp.Value.Level,
                        Exp = rrrp.Value.Exp
                    });
                }

                response.Data.LastTacticAcademyClass = user.CompletedTacticAcademyLessons.Max();
                response.Data.LastTacticAcademyLesson = user.CompletedTacticAcademyLessons.Max();
                response.Data.JukeboxCount = user.JukeboxBgm.Count;
                response.Data.CostumeLv = 1;
                response.Data.CostumeCount = GameData.Instance.CharacterCostumeTable.Count;

                if (user.ProfileRepresentativeFrame != null)
                {

                    response.Data.ProfileFrameHistoryType = user.ProfileRepresentativeFrame.ProfileFrameHistoryType;
                    foreach (int id in user.ProfileRepresentativeFrame.RecentAcquireFilterTypes)
                    {
                        response.Data.RecentAcquireFilterTypes.Add(id);
                    }
                    foreach (var data in user.ProfileRepresentativeFrame.UserProfileRepresentativeFrames)
                    {
                        response.Data.RepresentativeProfileFrames.Add(data);
                    }
                }

                foreach (KeyValuePair<int, UserFrameTableRecord> data in GameData.Instance.userFrameTable)
                {
                    response.Data.ProfileFrames.Add(new NetProfileFrameData() { FrameTid = data.Key, AcquiredAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddTicks(data.Key * 1000 - 11718490900)) });
                }
                response.Data.SimRoomOverclockHighScoreLevel = 1;
                // response.Data.SimRoomOverclockHighScoreData.Add(new NetProfileSimRoomOverclockHighScoreData() { });
                // response.Data.Desc = "这就是个测试";

                int slot = 0;
                foreach (long csn in user.RepresentationTeamDataNew)
                {
                    CharacterModel? c = user.GetCharacterBySerialNumber(csn);

                    if (c != null)
                    {
                        slot++;
                        response.Data.ProfileTeam.Add(new NetProfileTeamSlot() { Slot = slot, Default = new() { CostumeId = c.CostumeId, Csn = c.Csn, Grade = c.Grade, Lv = c.Level, Skill1Lv = c.Skill1Lvl, Skill2Lv = c.Skill2Lvl, Tid = c.Tid, UltiSkillLv = c.UltimateLevel } });
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
