using EpinelPS.Utils;
using EpinelPS.Data; // For GameData access

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitle/get")]
    public class GetUserTitle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserTitleList>();
            var r = new ResGetUserTitleList();

            // Access GameData and get all UserTitle IDs
            var userTitleRecords = GameData.Instance.userTitleRecords;

            foreach (var titleId in userTitleRecords.Keys)
            {
                r.UserTitleList.Add(new ResGetUserTitleList.Types.NetUserTitle() { UserTitleId = titleId });
            }

            await WriteDataAsync(r);
        }
    }
}
