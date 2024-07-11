using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetProfile")]
    public class GetUserProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetProfileData>();
            var user = GetUser();
            var response = new ResGetProfileData();
            response.Data = new NetProfileData();
            Console.WriteLine("GET USER PROFILE NOT IMPLEMENTED: " + req.TargetUsn);
            if (user.ID == (ulong)req.TargetUsn)
            {
                response.Data.User = LobbyHandler.CreateWholeUserDataFromDbUser(user);
                response.Data.LastActionAt = DateTimeOffset.UtcNow.Ticks;
                response.Data.CharacterCount = new() { Count = user.Characters.Count };
                response.Data.InfraCoreLv = user.InfraCoreLvl;
                response.Data.LastCampaignNormalStageId = user.LastNormalStageCleared;
                response.Data.LastCampaignHardStageId = user.LastHardStageCleared;
                response.Data.OutpostOpenState = user.MainQuestData.ContainsKey(25);
            }
            else
            {
                Logger.Warn("Unknown User ID: " + req.TargetUsn);
            }

            await WriteDataAsync(response);
        }
    }
}
