using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Controllers
{
    [Route("/v2")]
    [ApiController]
    public class LevelInfiniteControlller : Controller
    {
        private const string BadAuthToken = "{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"123" + "\"}";

        [HttpPost]
        [Route("conf/get_conf")]
        public string GetConfig(string sig)
        {
            return "{\"conf_version\":\"102\",\"msg\":\"\",\"ret\":1,\"seq\":\"" + sig + "\"}";
        }


        [HttpPost]
        [Route("auth/login")]
        public string AuthLogin(string seq, [FromBody] LoginEndpoint1Req req)
        {
            foreach (var tok in JsonDb.Instance.LauncherAccessTokens)
            {
                if (tok.Token == req.channel_info.account_token)
                {
                    var user = JsonDb.Instance.Users.Find(x => x.ID == tok.UserID);
                    if (user != null)
                    {
                        // todo: they use another token here, but we will reuse the same one.
                        // todo: use a class for this, this is a mess
                        return "{\"birthday\":\"1970-01\",\"channel_info\":{\"account\":\"" + user.Username + "\",\"account_plat_type\":131,\"account_token\":\"" + req.channel_info.account_token + "\",\"account_type\":1,\"account_uid\":\"" + user.ID + "\",\"expire_ts\":1721667004,\"is_login\":true,\"lang_type\":\"en\",\"phone_area_code\":\"\",\"token\":\"" + req.channel_info.account_token + "\"},\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"1719075066-0339089836-025921-1161847390\\\"}\",\"del_account_status\":0,\"del_li_account_status\":0,\"email\":\"" + user.Username + "\",\"extra_json\":{\"del_li_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + seq + "\\\"}\",\"get_status_rsp\":{\"adult_age\":14,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"724\",\"ret\":0,\"ts\":\"1719075065\"},\"need_notify_rsp\":{\"game_sacc_openid\":\"\",\"game_sacc_uid\":\"\",\"has_game_sacc_openid\":false,\"has_game_sacc_uid\":false,\"has_li_openid\":true,\"has_li_uid\":true,\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"li_openid\":\"" + user.ID + "\",\"li_uid\":\"2752409592679849\",\"need_notify\":false,\"user_agreed_game_dma\":\"2\",\"user_agreed_game_pp\":\"1\",\"user_agreed_game_tos\":\"1\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"1\",\"user_agreed_li_tos\":\"\"}},\"first_login\":0,\"gender\":0,\"msg\":\"success\",\"need_name_auth\":false,\"openid\":\"" + user.ID + "\",\"pf\":\"LevelInfinite_LevelInfinite-Windows-windows-Windows-LevelInfinite-09af79d65d6e4fdf2d2569f0d365739d-" + user.ID + "\",\"pf_key\":\"abc\",\"picture_url\":\"\",\"reg_channel_dis\":\"Windows\",\"ret\":0,\"seq\":\"29080-2d28ea26-d71f-4822-9118-0156f1e2dba4-1719075060-99\",\"token\":\"" + tok.Token + "\",\"token_expire_time\":" + tok.ExpirationTime + ",\"uid\":\"" + user.ID + "\",\"user_name\":\"" + user.PlayerName + "\"}";
                    }

                    break;
                }
            }

            // TODO: proper token expired message
            return "{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("auth/auto_login")]
        public string AutoLogin(string seq)
        {
            return "{\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + seq + "\\\"}\",\"del_account_status\":0,\"del_li_account_status\":0,\"extra_json\":{\"del_li_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + seq + "\\\"}\",\"get_status_msg\":\"success\",\"get_status_ret\":0,\"get_status_rsp\":{\"adult_age\":14,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"724\",\"ret\":0,\"ts\":\"" + DateTimeOffset.UtcNow.ToUnixTimeSeconds()
+ "\"},\"need_notify_msg\":\"success\",\"need_notify_ret\":0,\"need_notify_rsp\":{\"has_bind_li\":true,\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"user_agreed_game_dma\":\"2\",\"user_agreed_game_pp\":\"1\",\"user_agreed_game_tos\":\"1\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"1\",\"user_agreed_li_tos\":\"\"}},\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("minorcer/get_status")]
        public string MinorcerStatus(string seq)
        {
            return "{\"adult_age\":15,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"300\",\"ret\":0,\"seq\":\"" + seq + "\",\"ts\":\"1719156511\"}";
        }

        [HttpPost]
        [Route("profile/userinfo")]
        public string QueryUserInfo(string seq, [FromBody] AuthPkt2 req)
        {
            User? user;
            if ((user = NetUtils.GetUser(req.token).Item1) == null) return BadAuthToken;

            return "{\"bind_list\":[{\"channel_info\":{\"birthday\":\"1970-01\",\"email\":\"" + user.Username + "\",\"is_receive_email\":1,\"lang_type\":\"en\",\"last_login_time\":1719075003,\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"region\":\"724\",\"register_account\":\"" + user.Username + "\",\"register_account_type\":1,\"register_time\":" + user.RegisterTime + ",\"seq\":\"abc\",\"uid\":\"" + user.ID + "\",\"user_name\":\"" + user.PlayerName + "\",\"username_pass_verify\":0},\"channelid\":131,\"email\":\"" + user.Username + "\",\"picture_url\":\"\",\"user_name\":\"" + user.PlayerName + "\"}],\"birthday\":\"1970-01\",\"email\":\"" + user.Username + "\",\"gender\":0,\"msg\":\"success\",\"picture_url\":\"\",\"ret\":0,\"seq\":\"" + seq + "\",\"user_name\":\"" + user.PlayerName + "\"}";
        }

        [HttpPost]
        [Route("profile/query_account_info")]
        public string QueryAccountInfo(string seq, [FromBody] AuthPkt req)
        {
            User? user;
            if ((user = NetUtils.GetUser(req.channel_info.token).Item1) == null) return BadAuthToken;

            // Pretend that code is valid
            return "{\"game_sacc_openid\":\"\",\"game_sacc_uid\":\"\",\"has_game_sacc_openid\":false,\"has_game_sacc_uid\":false,\"has_li_openid\":false,\"has_li_uid\":true,\"is_receive_email\":-1,\"is_receive_email_in_night\":-1,\"li_openid\":\"\",\"li_uid\":\"" + user.ID + "\",\"msg\":\"success\",\"need_notify\":false,\"ret\":0,\"seq\":\"" + seq + "\",\"user_agreed_game_dma\":\"\",\"user_agreed_game_pp\":\"\",\"user_agreed_game_tos\":\"\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"\",\"user_agreed_li_tos\":\"\"}";
        }


        [HttpPost]
        [Route("reward/send")]
        public string SendDailyReward(string seq)
        {
            // Level infinite pass daily reward coints, not implemented as they are inaccessible currently
            return "{\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("profile/set_protocol")]
        public string SetProtocol(string seq)
        {
            // Enable encryption, not used in this server.
            return "{\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("notice/get_notice_content")]
        public string GetNotices(string seq)
        {
            return "{\r\n  \"msg\": \"success\",\r\n  \"notice_list\": [\r\n    {\r\n      \"app_id\": \"3001001\",\r\n      \"app_notice_id\": \"post-6rpvwgrdx1b\",\r\n      \"area_list\": \"[\\\"81\\\",\\\"82\\\",\\\"83\\\",\\\"84\\\",\\\"85\\\"]\",\r\n      \"content_list\": [\r\n        {\r\n          \"app_content_id\": \"post-9ilpu79xxzp\",\r\n          \"content\": \"This isn't working\",\r\n          \"extra_data\": \"{}\",\r\n          \"id\": 48706,\r\n          \"lang_type\": \"en\",\r\n          \"picture_list\": [\r\n            {\r\n              \"extra_data\": \"{\\\"id\\\":\\\"TitleImage\\\"}\",\r\n              \"hash\": \"44a99a61152b5b80a0466ff9f0cee2bc\",\r\n              \"redirect_url\": \"\",\r\n              \"url\": \"pnt-console-cdn.playernetwork.intlgame.com/prod/29080/notice/022681b1121a40259a575fbe587651b4.jpg\"\r\n            }\r\n          ],\r\n          \"title\": \"New Character\",\r\n          \"update_time\": 1717637493\r\n        }\r\n      ],\r\n      \"end_time\": 1819431999,\r\n      \"extra_data\": \"{\\\"NoticeType\\\":\\\"Event\\\",\\\"Order\\\":\\\"11\\\",\\\"extra_reserved\\\":\\\"{\\\\\\\"Author\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"Category\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"CreateType\\\\\\\":\\\\\\\"4\\\\\\\",\\\\\\\"IsOpenService\\\\\\\":\\\\\\\"0\\\\\\\",\\\\\\\"IsToping\\\\\\\":true,\\\\\\\"Keyword\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"Sort\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"TopEnd\\\\\\\":\\\\\\\"2030-01-01 00:00:01\\\\\\\",\\\\\\\"TopStart\\\\\\\":\\\\\\\"2000-01-01 00:00:01\\\\\\\"}\\\"}\",\r\n      \"id\": 7560,\r\n      \"picture_list\": [],\r\n      \"start_time\": 1717617599,\r\n      \"status\": 1,\r\n      \"update_time\": 1717637494\r\n    }\r\n  ],\r\n  \"ret\": 0,\r\n  \"seq\": \"" + seq + "\"\r\n}";
        }

        [HttpPost]
        [Route("lbs/ipregion")]
        public string GetIpRegion(string seq)
        {
            return "{\"alpha2\":\"GR\",\"extra_json\":{\"certificate_type_map\":{}},\"msg\":\"success\",\"region\":\"300\",\"ret\":0,\"seq\":\"" + seq + "\",\"timestamp\":324234322}";
        }

        [HttpPost]
        [Route("gnconfig/acquire_config")]
        public string AcquireConfig(string seq)
        {
            return "{\"ret\":23111202,\"msg\":\"no matched config error( [match logic]no match )\",\"rule_id\":\"\",\"resource_list\":\"\",\"sdk_enable\":0,\"sdk_debug_enable\":0,\"report_log_enable\":0,\"log_level\":0,\"inner_seq\":\"" + seq + "\",\"ab_test\":{\"id\":\"\",\"group\":\"\"},\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("profile/get_bind_info")]
        public string GetProfileBindInfo(string seq, [FromBody] AuthPkt req)
        {
            User? user;
            if ((user = NetUtils.GetUser(req.channel_info.token).Item1) == null) return BadAuthToken;

            return "{\"bind_list\":[{\"bind_ts\":1717783095,\"channel_info\":{\"birthday\":\"1970-01\",\"email\":\"" + user.Username + "\",\"is_receive_email\":1,\"lang_type\":\"en\",\"last_login_time\":171000000,\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"region\":\"724\",\"register_account\":\"" + user.Username + "\",\"register_account_type\":1,\"register_time\":" + user.RegisterTime + ",\"seq\":\"" + seq + "\",\"uid\":\"2752409592679849\",\"user_name\":\"" + user.PlayerName + "\",\"username_pass_verify\":0},\"channelid\":131,\"email\":\"" + user.Username + "\",\"history_scopes\":[],\"is_primary\":1,\"picture_url\":\"\",\"user_name\":\"" + user.PlayerName + "\"}],\"create_ts\":" + user.RegisterTime + ",\"last_login_ts\":171000000,\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }
    }
}
