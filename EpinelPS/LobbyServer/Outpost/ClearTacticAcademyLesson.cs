using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/tactic/clearlesson")]
    public class ClearTacticAcademyLesson : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqTacticAcademyClearLesson req = await ReadData<ReqTacticAcademyClearLesson>();
            User user = GetUser();

            ResTacticAcademyClearLesson response = new()
            {
                ClearLessonTid = req.LessonTid
            };

            TacticAcademyFunctionRecord x = GameData.Instance.GetTacticAcademyLesson(req.LessonTid);

            if (user.CanSubtractCurrency((CurrencyType)x.CurrencyId, x.CurrencyValue))
            {
                user.SubtractCurrency((CurrencyType)x.CurrencyId, x.CurrencyValue);

                user.CompletedTacticAcademyLessons.Add(req.LessonTid);

                ProcessLessonReward(user, x);

                foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
                {
                    response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
                }
                JsonDb.Save();
            }
            else
            {
                Console.WriteLine($"User {user.PlayerName} tried to clear lesson {req.LessonTid} without enough currency");
            }
            await WriteDataAsync(response);
        }

        private static void ProcessLessonReward(User user, TacticAcademyFunctionRecord r)
        {
            if (r.LessonReward == null)
            {
                Console.WriteLine("Warning: lesson_reward shouldnt be null");
                return;
            }

            if (r.LessonType == LessonType.OutpostBattle)
            {
                foreach (var item in r.LessonReward)
                {
                    if (item.LessonRewardId != 0 && item.LessonRewardValue != 0)
                    {
                        user.OutpostBuffs.GetPercentages((CurrencyType)item.LessonRewardId).Add(item.LessonRewardValue);
                    }
                }
            }
            else
            {
                Console.WriteLine("Warning: unknown lesson type: " + r.LessonType);
            }
        }
    }
}
