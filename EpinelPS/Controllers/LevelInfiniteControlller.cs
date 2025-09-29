using System.Reflection;
using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

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
            foreach (AccessToken tok in JsonDb.Instance.LauncherAccessTokens)
            {
                if (tok.Token == req.channel_info.account_token)
                {
                    User? user = JsonDb.Instance.Users.Find(x => x.ID == tok.UserID);
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
        public string AutoLogin(string seq, [FromBody] AuthPkt2 req)
        {
            User? user;
            if ((user = NetUtils.GetUser(req.token).Item1) == null) return BadAuthToken;


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
        private static IntlNotice CreateNotice(int id, NoticeType type, string contentText, string title = "", string picture = "")
        {
            IntlNotice notice = new()
            {
                app_id = "3001001",
                app_notice_id = "post-" + id,
                area_list = "[\"81\",\"82\",\"83\",\"84\",\"85\"]",
                extra_data = "{\"NoticeType\":\"" + type.ToString() + "\",\"Order\":\"11\",\"extra_reserved\":\"{\\\"Author\\\":\\\"\\\",\\\"Category\\\":\\\"\\\",\\\"CreateType\\\":\\\"4\\\",\\\"IsOpenService\\\":\\\"0\\\",\\\"IsToping\\\":true,\\\"Keyword\\\":\\\"\\\",\\\"Sort\\\":\\\"\\\",\\\"TopEnd\\\":\\\"2030-01-01 00:00:01\\\",\\\"TopStart\\\":\\\"2000-01-01 00:00:01\\\"}\"}",
                id = id,
                start_time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                end_time = (int)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                update_time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                status = 1
            };

            ContentList content = new()
            {
                app_content_id = "post-" + id,
                content = contentText,
                extra_data = "{}",
                id = id,
                lang_type = "en",
                title = title,
                update_time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            if (!string.IsNullOrEmpty(picture))
            {
                content.picture_list.Add(new PictureList()
                {
                    extra_data = "{\"id\":\"TitleImage\"}",
                    hash = "",
                    redirect_url = "",
                    url = picture
                });
            }

            notice.content_list.Add(content);
            return notice;
        }

        [HttpPost]
        [Route("notice/get_notice_content")]
        public IntlNoticeListResponse GetNotices(string seq)
        {
            IntlNoticeListResponse rsp = new()
            {
                seq = seq,
                ret = 0,
                msg = "success"
            };

            rsp.notice_list.Add(CreateNotice(2, NoticeType.System, "You are running EpinelPS v" + Assembly.GetExecutingAssembly().GetName().Version, "Server version"));

            return rsp;
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
        public string GetProfileBindInfo(string seq, [FromBody] AuthPkt2 req)
        {
            User? user;
            if ((user = NetUtils.GetUser(req.token).Item1) == null) return BadAuthToken;

            return "{\"bind_list\":[{\"bind_ts\":1717783095,\"channel_info\":{\"birthday\":\"1970-01\",\"email\":\"" + user.Username + "\",\"is_receive_email\":1,\"lang_type\":\"en\",\"last_login_time\":171000000,\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"region\":\"724\",\"register_account\":\"" + user.Username + "\",\"register_account_type\":1,\"register_time\":" + user.RegisterTime + ",\"seq\":\"" + seq + "\",\"uid\":\"2752409592679849\",\"user_name\":\"" + user.PlayerName + "\",\"username_pass_verify\":0},\"channelid\":131,\"email\":\"" + user.Username + "\",\"history_scopes\":[],\"is_primary\":1,\"picture_url\":\"\",\"user_name\":\"" + user.PlayerName + "\"}],\"create_ts\":" + user.RegisterTime + ",\"last_login_ts\":171000000,\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("auth/refresh_sacc_token")]
        public string RefreshAuthToken(string seq, [FromBody] AuthPkt2 req)
        {
            // TODO redo auth token system
            AccessToken? user;
            if ((user = NetUtils.GetUser(req.token).Item2) == null) return BadAuthToken;
            user.ExpirationTime = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds();

            return "{\"msg\":\"success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }
    }
}
