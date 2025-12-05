using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/get")]
    public class GetCharacterData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCharacterData req = await ReadData<ReqGetCharacterData>();
            User user = GetUser();

            ResGetCharacterData response = new();
            foreach (CharacterModel item in user.Characters)
            {
                response.Character.Add(new NetUserCharacterData()
                {
                    Default = new()
                    {
                        Csn = item.Csn,
                        Skill1Lv = item.Skill1Lvl,
                        Skill2Lv = item.Skill2Lvl,
                        CostumeId = item.CostumeId,
                        Lv = user.GetCharacterLevel(item.Csn, item.Level),
                        Grade = item.Grade,
                        Tid = item.Tid,
                        UltiSkillLv = item.UltimateLevel
                    },
                    IsSynchro = user.GetSynchro(item.Csn)
                });
                
                // Check if character is main force
                if (item.IsMainForce)
                {
                    response.MainForceCsnList.Add(item.Csn);
                }
            }

            List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

            foreach (CharacterModel? c in highestLevelCharacters)
            {
                response.SynchroStandardCharacters.Add(c.Csn);
            }

            await WriteDataAsync(response);
        }
    }
}
