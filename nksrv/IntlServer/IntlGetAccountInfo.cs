namespace nksrv.IntlServer
{
    internal class IntlGetAccountInfo : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            if (User == null || UsedToken == null)
                throw new Exception("no user"); // should never happen

            await WriteJsonStringAsync("{\"account_type\":1,\"birthday\":\"1970-01\",\"email\":\"" + User.Username + "\",\"expire\":" + UsedToken.ExpirationTime + ",\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"is_receive_video\":-1,\"lang_type\":\"en\",\"msg\":\"Success\",\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"privacy_policy\":\"1\",\"privacy_update_time\":1717783097,\"region\":\"724\",\"ret\":0,\"seq\":\"" + Seq + "\",\"terms_of_service\":\"\",\"terms_update_time\":0,\"uid\":\"" + User.ID + "\",\"user_agreed_dt\":\"\",\"user_agreed_pp\":\"1\",\"user_agreed_tos\":\"\",\"user_name\":\"" + User.PlayerName + "\",\"username_pass_verify\":0}");
        }
    }
}
