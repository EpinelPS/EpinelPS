using EmbedIO;
using nksrv.Utils;

namespace nksrv.IntlServer
{
    public static class IntlHandler
    {
        public static Dictionary<string, IntlMsgHandler> Handlers = new Dictionary<string, IntlMsgHandler>()
        {
            {"/login", new IntlLogin1Endpoint() }, // /account/login
            {"/auth/login", new IntlLogin2Endpoint() }, // /v2/auth/login
            {"/sendcode", new SendCodeEndpoint() }, // /account/sendcode
            {"/codestatus", new CodeStatusEndpoint() }, // 
            {"/register", new IntlAccountRegisterEndpoint() }, // /account/register
            {"/profile/query_account_info", new IntlQueryAccountInfo() }, // /account/register
            {"/conf/get_conf", new IntlReturnJsonHandler(GetConfResp) }, // /v2/conf/get_conf
            {"/minorcer/get_status", new IntlReturnJsonHandler(MinorcerResp) }, // /v2/minorcer/get_status
            {"/profile/set_protocol", new IntlReturnJsonHandler(SetProtocolResp) },
            {"/profile/userinfo", new IntlGetProfileInfo() },
            {"/getuserinfo", new IntlGetAccountInfo() },
            {"/lbs/ipregion", new IntlReturnJsonHandler(IpRegionResp) },
            {"/profile/get_bind_info", new IntlGetProfileBindInfo() },
            {"/gnconfig/acquire_config", new IntlReturnJsonHandler(AquireConfigResp) },
            {"/auth/auto_login", new AutoLoginEndpoint() },
            {"/reward/send", new IntlReturnJsonHandler(SetProtocolResp) }, // /v2/reward/send
            {"/notice/get_notice_content", new GetNoticeContent() }, // /v2/notice/get_notice_content
            {"/fleet.repo.game.RepoSVC/GetVersion", new JuniperLauncherGetRepoVersion() }, // /api/v1/fleet.repo.game.RepoSVC/GetVersion
            {"/fleet.repo.game.RepoMgr/GetGameLauncher", new JuniperLauncherGetGameLauncher() }, // /api/v1/fleet.repo.game.RepoMgr/
            {"/fleet.repo.game.RepoSVC/GetRegion", new JuniperLauncherGetRegion() }, // /api/v1/fleet.repo.game.RepoMgr/                                                     // GetGameLauncher
            {"/fleet.auth.game.AuthSvr/Login", new JupiterAuthLogin() } // /api/v1/fleet.auth.game.AuthSvr/Login
        };
        public const string GetConfResp = "{\"conf_version\":\"102\",\"msg\":\"\",\"ret\":1,\"seq\":\"((SEGID))\"}";
        public const string MinorcerResp = "{\"adult_age\":15,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"300\",\"ret\":0,\"seq\":\"((SEGID))\",\"ts\":\"1719156511\"}";
        public const string SetProtocolResp = "{\"msg\":\"success\",\"ret\":0,\"seq\":\"((SEGID))\"}";
        public const string IpRegionResp = "{\"alpha2\":\"GR\",\"extra_json\":{\"certificate_type_map\":{}},\"msg\":\"success\",\"region\":\"300\",\"ret\":0,\"seq\":\"((SEGID))\",\"timestamp\":324234322}";
        public const string AquireConfigResp = "{\"ret\":23111202,\"msg\":\"no matched config error( [match logic]no match )\",\"rule_id\":\"\",\"resource_list\":\"\",\"sdk_enable\":0,\"sdk_debug_enable\":0,\"report_log_enable\":0,\"log_level\":0,\"inner_seq\":\"((SEGID))\",\"ab_test\":{\"id\":\"\",\"group\":\"\"},\"seq\":\"((SEGID))\"}";
        public static async Task Handle(IHttpContext context)
        {
            IntlMsgHandler? handler = null;
            foreach (var item in Handlers)
            {
                if (context.RequestedPath == item.Key)
                {
                    handler = item.Value;
                }
            }

            if (handler == null)
            {
                context.Response.StatusCode = 404;
            }
            else
            {
                handler.Reset();
                await handler.HandleAsync(context);
            }
        }

        public static AccessToken CreateLauncherTokenForUser(User user)
        {
            // TODO: implement access token expiration
            AccessToken token = new() { ExpirationTime = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds() };
            token.Token = Rng.RandomString(64);
            token.UserID = user.ID;
            JsonDb.Instance.LauncherAccessTokens.Add(token);
            JsonDb.Save();

            return token;
        }
    }
}
