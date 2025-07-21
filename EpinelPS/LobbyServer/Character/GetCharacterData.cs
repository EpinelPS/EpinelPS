using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/get")]
    public class GetCharacterData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCharacterData req = await ReadData<ReqGetCharacterData>();
            Database.User user = GetUser();

            ResGetCharacterData response = new();
            foreach (Database.Character item in user.Characters)
            {
                response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = user.GetCharacterLevel(item.Csn, item.Level), Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });
            }

            List<Database.Character> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

            foreach (Database.Character? c in highestLevelCharacters)
            {
                response.SynchroStandardCharacters.Add(c.Csn);
            }

            await WriteDataAsync(response);
        }
    }
}
