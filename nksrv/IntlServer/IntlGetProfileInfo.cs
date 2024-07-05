using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    public class IntlGetProfileInfo : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            if (User == null)
                throw new Exception("no user"); // should never happen

            await WriteJsonStringAsync("{\"bind_list\":[{\"channel_info\":{\"birthday\":\"1970-01\",\"email\":\""+User.Username+"\",\"is_receive_email\":1,\"lang_type\":\"en\",\"last_login_time\":1719075003,\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"region\":\"724\",\"register_account\":\""+User.Username+"\",\"register_account_type\":1,\"register_time\":"+User.RegisterTime+",\"seq\":\"abc\",\"uid\":\""+User.ID+"\",\"user_name\":\""+User.PlayerName+"\",\"username_pass_verify\":0},\"channelid\":131,\"email\":\"" + User.Username + "\",\"picture_url\":\"\",\"user_name\":\""+User.PlayerName+"\"}],\"birthday\":\"1970-01\",\"email\":\"" + User.Username + "\",\"gender\":0,\"msg\":\"success\",\"picture_url\":\"\",\"ret\":0,\"seq\":\"" + Seq + "\",\"user_name\":\"" + User.PlayerName + "\"}");
        }
    }
}
