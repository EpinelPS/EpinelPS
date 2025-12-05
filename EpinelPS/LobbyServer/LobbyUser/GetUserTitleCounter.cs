using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitlecounter/get")]
    public class GetUserTitleCounter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetUserTitleCounterList>();

            ResGetUserTitleCounterList response = new();
            response.UserTitleCounterList.Add(new ResGetUserTitleCounterList.Types.NetUserTitleCounter { Condition = 23, SubCondition = 1, Count = 10});
            response.UserTitleCounterList.Add(new ResGetUserTitleCounterList.Types.NetUserTitleCounter { Condition = 23, SubCondition = 2, Count = 10});
            response.UserTitleCounterList.Add(new ResGetUserTitleCounterList.Types.NetUserTitleCounter { Condition = 23, SubCondition = 3, Count = 10});
            response.UserTitleCounterList.Add(new ResGetUserTitleCounterList.Types.NetUserTitleCounter { Condition = 23, SubCondition = 4, Count = 10});
            response.UserTitleCounterList.Add(new ResGetUserTitleCounterList.Types.NetUserTitleCounter { Condition = 23, SubCondition = 5, Count = 10});

            await WriteDataAsync(response);
        }
    }
}
