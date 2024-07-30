using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/get")]
    public class GetCharacterData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCharacterData>();
            var user = GetUser();

            var response = new ResGetCharacterData();
            // TODO: When Squad view opens in the game, or this request is sent, all character levels reset to 1 as well as cusume IDs
            //foreach (var item in user.Characters)
            //{
            //    response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = false, Artifact = 0 });
            //}

            var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

            foreach (var c in highestLevelCharacters)
            {
                response.SynchroStandardCharacters.Add(c.Csn);
            }

            await WriteDataAsync(response);
        }
    }
}
