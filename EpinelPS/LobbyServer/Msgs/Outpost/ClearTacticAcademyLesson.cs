using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using Swan.Logging;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/tactic/clearlesson")]
    public class ClearTacticAcademyLesson : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqTacticAcademyClearLesson>();
            var user = GetUser();

            var response = new ResTacticAcademyClearLesson();
            response.ClearLessonTid = req.LessonTid;

            var x = GameData.Instance.GetTacticAcademyLesson(req.LessonTid);

            if (user.CanSubtractCurrency(x.CurrencyId, x.CurrencyValue))
            {
                user.SubtractCurrency(x.CurrencyId, x.CurrencyValue);

                user.CompletedTacticAcademyLessons.Add(req.LessonTid);

                foreach (var currency in user.Currency)
                {
                    response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
                }
            }
            else
            {
                Logger.Error($"User {user.PlayerName} tried to clear lesson {req.LessonTid} without enough currency");
            }
            await WriteDataAsync(response);
        }
    }
}
