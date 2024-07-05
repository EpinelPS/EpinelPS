using EmbedIO;
using Newtonsoft.Json;
using nksrv.Utils;
using System.Net;

namespace nksrv.IntlServer
{
    /// <summary>
    /// This handles the login endpoint.
    /// </summary>
    public class IntlLogin2Endpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            LoginEndpoint1Req? ep = JsonConvert.DeserializeObject<LoginEndpoint1Req>(Content);
            if (ep != null)
            {
                foreach (var tok in JsonDb.Instance.LauncherAccessTokens)
                {
                    if (tok.Token == ep.channel_info.account_token)
                    {
                        var user = JsonDb.Instance.Users.Find(x => x.ID == tok.UserID);
                        if (user != null)
                        {
                            // todo: they use another token here, but we will reuse the same one.
                            await WriteJsonStringAsync("{\"birthday\":\"1970-01\",\"channel_info\":{\"account\":\"" + user.Username + "\",\"account_plat_type\":131,\"account_token\":\"" + ep.channel_info.account_token + "\",\"account_type\":1,\"account_uid\":\"" + user.ID + "\",\"expire_ts\":1721667004,\"is_login\":true,\"lang_type\":\"en\",\"phone_area_code\":\"\",\"token\":\"" + ep.channel_info.account_token + "\"},\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"1719075066-0339089836-025921-1161847390\\\"}\",\"del_account_status\":0,\"del_li_account_status\":0,\"email\":\"" + user.Username + "\",\"extra_json\":{\"del_li_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"1719075065-4128751114-032271-2064970828\\\"}\",\"get_status_rsp\":{\"adult_age\":14,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"724\",\"ret\":0,\"ts\":\"1719075065\"},\"need_notify_rsp\":{\"game_sacc_openid\":\"\",\"game_sacc_uid\":\"\",\"has_game_sacc_openid\":false,\"has_game_sacc_uid\":false,\"has_li_openid\":true,\"has_li_uid\":true,\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"li_openid\":\"43599204002070510000\",\"li_uid\":\"2752409592679849\",\"need_notify\":false,\"user_agreed_game_dma\":\"2\",\"user_agreed_game_pp\":\"1\",\"user_agreed_game_tos\":\"1\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"1\",\"user_agreed_li_tos\":\"\"}},\"first_login\":0,\"gender\":0,\"msg\":\"success\",\"need_name_auth\":false,\"openid\":\"43599204002070510000\",\"pf\":\"LevelInfinite_LevelInfinite-Windows-windows-Windows-LevelInfinite-09af79d65d6e4fdf2d2569f0d365739d-43599204002070510000\",\"pf_key\":\"abc\",\"picture_url\":\"\",\"reg_channel_dis\":\"Windows\",\"ret\":0,\"seq\":\"29080-2d28ea26-d71f-4822-9118-0156f1e2dba4-1719075060-99\",\"token\":\"" + tok.Token + "\",\"token_expire_time\":" + tok.ExpirationTime + ",\"uid\":\"" + user.ID + "\",\"user_name\":\"" + user.PlayerName + "\"}");
                            return;
                        }

                        break;

                        throw new HttpException(HttpStatusCode.MultipleChoices);
                    }
                }
            }
            else
            {
                throw new HttpException(HttpStatusCode.BadRequest);
            }
        }

        public class DeviceInfo
        {
            public string guest_id { get; set; } = "";
            public string lang_type { get; set; } = "";
            public string root_info { get; set; } = "";
            public string app_version { get; set; } = "";
            public string screen_dpi { get; set; } = "";
            public int screen_height { get; set; }
            public int screen_width { get; set; }
            public string device_brand { get; set; } = "";
            public string device_model { get; set; } = "";
            public int network_type { get; set; }
            public int ram_total { get; set; }
            public int rom_total { get; set; }
            public string cpu_name { get; set; } = "";
            public string client_region { get; set; } = "";
            public string vm_type { get; set; } = "";
            public string xwid { get; set; } = "";
            public string new_xwid { get; set; } = "";
            public string xwid_flag { get; set; } = "";
            public string cpu_arch { get; set; } = "";
        }

        public class LoginEndpoint1Req
        {
            public ChannelInfo channel_info { get; set; } = new();
            public DeviceInfo device_info { get; set; } = new();
            public string channel_dis { get; set; } = "";
            public string login_extra_info { get; set; } = "";
            public string lang_type { get; set; } = "";
        }
    }
}
