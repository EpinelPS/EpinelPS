using EmbedIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    internal class IntlQueryAccountInfo : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            if (User == null) throw new Exception("user cannot be null");

            // doesnt seem to be important, send some data
            await WriteJsonStringAsync("{\"game_sacc_openid\":\"\",\"game_sacc_uid\":\"\",\"has_game_sacc_openid\":false,\"has_game_sacc_uid\":false,\"has_li_openid\":false,\"has_li_uid\":true,\"is_receive_email\":-1,\"is_receive_email_in_night\":-1,\"li_openid\":\"\",\"li_uid\":\"" + User.ID + "\",\"msg\":\"success\",\"need_notify\":false,\"ret\":0,\"seq\":\"" + Seq + "\",\"user_agreed_game_dma\":\"\",\"user_agreed_game_pp\":\"\",\"user_agreed_game_tos\":\"\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"\",\"user_agreed_li_tos\":\"\"}");
        }
    }
}
