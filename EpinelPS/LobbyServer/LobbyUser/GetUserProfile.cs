namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/GetProfile")]
public class GetUserProfile : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetProfileData req = await ReadData<ReqGetProfileData>();
        User callingUser = GetUser();
        User? user = GetUser((ulong)req.TargetUsn);
        GameUser? userNew = GetUserNew((ulong)req.TargetUsn);
        ResGetProfileData response = new();

        if (user != null && userNew != null)
        {
            response.Data = new NetProfileData
            {
                User = LobbyHandler.CreateWholeUserDataFromDbUser(userNew),
                LastActionAt = DateTimeOffset.UtcNow.Ticks,
            };
            response.Data.CharacterCount.Add(new NetCharacterCount() { Count = user.Characters.Count });
            response.Data.InfraCoreLv = userNew.InfraCoreLvl;
            response.Data.LastCampaignNormalStageId = userNew.LastNormalStageCleared;
            response.Data.LastCampaignHardStageId = userNew.LastHardStageCleared;
            response.Data.OutpostOpenState = user.MainQuestData.ContainsKey(25);

            for (int i = 0; i < user.RepresentationTeamDataNew.Length; i++)
            {
                long csn = user.RepresentationTeamDataNew[i];
                CharacterModel? c = user.GetCharacterBySerialNumber(csn);

                if (c != null)
                {
                    response.Data.ProfileTeam.Add(new NetProfileTeamSlot() { Slot = i + 1, Default = new() { CostumeId = c.CostumeId, Csn = c.Csn, Grade = c.Grade, Lv = c.Level, Skill1Lv = c.Skill1Lvl, Skill2Lv = c.Skill2Lvl, Tid = c.Tid, UltiSkillLv = c.UltimateLevel } });
                }
            }
        }

        await WriteDataAsync(response);
    }
}
