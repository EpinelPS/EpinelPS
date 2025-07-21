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

            TacticAcademyLessonRecord x = GameData.Instance.GetTacticAcademyLesson(req.LessonTid);

            if (user.CanSubtractCurrency((CurrencyType)x.currency_id, x.currency_value))
            {
                user.SubtractCurrency((CurrencyType)x.currency_id, x.currency_value);

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

        private static void ProcessLessonReward(User user, TacticAcademyLessonRecord r)
        {
            if (r.lesson_reward == null)
            {
                Console.WriteLine("Warning: lesson_reward shouldnt be null");
                return;
            }

            if (r.lesson_type == "OutpostBattle")
            {
                foreach (TacticAcademyLessonReward item in r.lesson_reward)
                {
                    if (item.lesson_reward_id != 0 && item.lesson_reward_value != 0)
                    {
                        user.OutpostBuffs.GetPercentages((CurrencyType)item.lesson_reward_id).Add(item.lesson_reward_value);
                    }
                }
            }
            else
            {
                Console.WriteLine("Warning: unknown lesson type: " + r.lesson_type);
            }
        }
    }
}
