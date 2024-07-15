using nksrv.Net;
using nksrv.StaticInfo;
using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/tactic/clearlesson")]
    public class ClearTacticAcademyLesson : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<TacticAcademyClearLessonRequest>();
            var user = GetUser();

            var response = new TacticAcademyClearLessonResponse();
            response.LessonId = req.LessonId;

            var x = StaticDataParser.Instance.GetTacticAcademyLesson(req.LessonId);

            if (user.CanSubtractCurrency(x.CurrencyId, x.CurrencyValue))
            {
                user.SubtractCurrency(x.CurrencyId, x.CurrencyValue);

                user.CompletedTacticAcademyLessons.Add(req.LessonId);

                foreach (var currency in user.Currency)
                {
                    response.RemainingCurrency.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
                }
            }
            else
            {
                Logger.Error($"User {user.PlayerName} tried to clear lesson {req.LessonId} without enough currency");
            }
            await WriteDataAsync(response);
        }
    }
}
