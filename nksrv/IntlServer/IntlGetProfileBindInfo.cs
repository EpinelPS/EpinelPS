
namespace nksrv.IntlServer
{
    internal class IntlGetProfileBindInfo : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            if (User == null)
                throw new Exception("no user"); // should never happen


            // TODO: last login time, but is it needed?
            await WriteJsonStringAsync("{\"bind_list\":[{\"bind_ts\":1717783095,\"channel_info\":{\"birthday\":\"1970-01\",\"email\":\"" + User.Username + "\",\"is_receive_email\":1,\"lang_type\":\"en\",\"last_login_time\":171000000,\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"region\":\"724\",\"register_account\":\"" + User.Username + "\",\"register_account_type\":1,\"register_time\":" + User.RegisterTime + ",\"seq\":\"" + Seq + "\",\"uid\":\"2752409592679849\",\"user_name\":\"" + User.PlayerName + "\",\"username_pass_verify\":0},\"channelid\":131,\"email\":\"" + User.Username + "\",\"history_scopes\":[],\"is_primary\":1,\"picture_url\":\"\",\"user_name\":\"" + User.PlayerName + "\"}],\"create_ts\":" + User.RegisterTime + ",\"last_login_ts\":171000000,\"msg\":\"success\",\"ret\":0,\"seq\":\"" + Seq + "\"}");
        }
    }
}