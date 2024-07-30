using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/execute")]
    public class ExecGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExecuteGacha>();
            var user = GetUser();

            var response = new ResExecuteGacha();

            // TODO: Pick random character that player does not have unless it supports limit break.

            // TODO implement reward
            response.Reward = new NetRewardData();

            foreach (var c in GameData.Instance.GetAllCharacterTids())
            {
                if (!user.HasCharacter(c))
                {
                    var id = user.GenerateUniqueCharacterId();
                    response.Gacha.Add(new NetGachaEntityData() { Corporation = 1, PieceCount = 1, CurrencyValue = 5, Sn = id, Tid = c, Type = 1 });

                    response.Characters.Add(new NetUserCharacterDefaultData() { CostumeId = 0, Csn = id, Grade = 0, Lv = 1, Skill1Lv = 1, Skill2Lv = 1, Tid = c, UltiSkillLv = 1 });

                    user.Characters.Add(new Database.Character() { CostumeId = 0, Csn = id, Grade = 0, Level = 1, Skill1Lvl = 1, Skill2Lvl = 1, Tid = c, UltimateLevel = 1 });
                }
                else
                {
                    // TODO add spare body
                }
            }
            user.GachaTutorialPlayCount++;

            JsonDb.Save();


            await WriteDataAsync(response);
        }
    }
}
