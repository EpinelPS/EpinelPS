using EpinelPS.Utils;
using Swan.Logging;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetProfile")]
    public class GetUserProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetProfileData>();
            var callingUser = GetUser();
            var user = GetUser((ulong)req.TargetUsn);
            var response = new ResGetProfileData();
          
            if (user != null)
            {
                response.Data = new NetProfileData();
                response.Data.User = LobbyHandler.CreateWholeUserDataFromDbUser(user);
                response.Data.LastActionAt = DateTimeOffset.UtcNow.Ticks;
                response.Data.CharacterCount.Add(new NetCharacterCount() { Count = user.Characters.Count });
                response.Data.InfraCoreLv = user.InfraCoreLvl;
                response.Data.LastCampaignNormalStageId = user.LastNormalStageCleared;
                response.Data.LastCampaignHardStageId = user.LastHardStageCleared;
                response.Data.OutpostOpenState = user.MainQuestData.ContainsKey(25);

                foreach (var item in user.RepresentationTeamData.Slots)
                {
                    var c = user.GetCharacterBySerialNumber(item.Csn);

                    if (c != null)
                    {
                        response.Data.ProfileTeam.Add(new NetProfileTeamSlot() { Slot = item.Slot, Default = new() { CostumeId = c.CostumeId, Csn = c.Csn, Grade = c.Grade, Level = c.Level, Skill1Lv = c.Skill1Lvl, Skill2Lv = c.Skill2Lvl, Tid = c.Tid, UltiSkillLv = c.UltimateLevel } });
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
