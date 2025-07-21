using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetProfile")]
    public class GetUserProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetProfileData req = await ReadData<ReqGetProfileData>();
            Database.User callingUser = GetUser();
            Database.User? user = GetUser((ulong)req.TargetUsn);
            ResGetProfileData response = new();
          
            if (user != null)
            {
                response.Data = new NetProfileData
                {
                    User = LobbyHandler.CreateWholeUserDataFromDbUser(user),
                    LastActionAt = DateTimeOffset.UtcNow.Ticks,
                };
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = user.Characters.Count });
                response.Data.InfraCoreLv = user.InfraCoreLvl;
                response.Data.LastCampaignNormalStageId = user.LastNormalStageCleared;
                response.Data.LastCampaignHardStageId = user.LastHardStageCleared;
                response.Data.OutpostOpenState = user.MainQuestData.ContainsKey(25);

                for (int i = 0; i < user.RepresentationTeamDataNew.Length; i++)
                {
                    long csn = user.RepresentationTeamDataNew[i];
                    Database.Character? c = user.GetCharacterBySerialNumber(csn);

                    if (c != null)
                    {
                        response.Data.ProfileTeam.Add(new NetProfileTeamSlot() { Slot = i + 1, Default = new() { CostumeId = c.CostumeId, Csn = c.Csn, Grade = c.Grade, Lv = c.Level, Skill1Lv = c.Skill1Lvl, Skill2Lv = c.Skill2Lvl, Tid = c.Tid, UltiSkillLv = c.UltimateLevel } });
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
