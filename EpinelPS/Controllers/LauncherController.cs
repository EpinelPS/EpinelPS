using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Controllers
{
    [Route("/api/v1")]
    [ApiController]
    public class LauncherController : Controller
    {
        [HttpPost]
        [Route("fleet.auth.game.AuthSvr/Login")]
        public string LauncherLogin()
        {
            return @"{
    ""result"": {
        ""error_code"": 0,
        ""error_message"": ""COMM_SUCC""
    },
    ""channel"": 0,
    ""game_id"": ""0"",
    ""openid"": """",
    ""uid"": """",
    ""biz_ticket"": """",
    ""expire_interval"": 0,
    ""refresh_interval"": 0,
    ""login_key"": """",
    ""login_ticket"": """",
    ""third_uid"": """"
}";
        }

        [HttpPost]
        [Route("fleet.repo.game.RepoSVC/GetRegion")]
        public string LauncherGetRegion()
        {
            return @"{
    ""result"": {
        ""error_code"": 0,
        ""error_message"": ""success""
    },
    ""region_info"": [
        {
            ""game_id"": ""16601"",
            ""region_id"": ""10001"",
            ""region_name_en_us"": ""Global"",
            ""region_name_i18n"": """",
            ""region_description_en_us"": ""Nikke Global Version"",
            ""region_description_i18n"": """",
            ""bind_branches"": """",
            ""meta_data"": """",
            ""sequence"": 0,
            ""status"": 2,
            ""branch_info"": [
                {
                    ""game_id"": ""16601"",
                    ""branch_id"": ""1"",
                    ""branch_name"": ""Official_release"",
                    ""branch_type"": 0,
                    ""description"": ""正式发布环境 release包""
                }
            ]
        }
    ]
}";
        }

        [HttpPost]
        [Route("fleet.repo.game.RepoSVC/GetGameLauncher")]
        public string LauncherGetLauncher()
        {
            return @"{
    ""result"": {
        ""error_code"": 0,
        ""error_message"": ""COMM_SUCC""
    },
    ""game_launcher_info"": [
        {
            ""id"": 27,
            ""execute_file"": ""NIKKE\\Game\\NIKKE.exe"",
            ""param"": """",
            ""description"": ""Nikke main process"",
            ""os"": ""any"",
            ""branch_id"": 0,
            ""status"": 1,
            ""param_type"": 1
        }
    ]
}";
        }

        [HttpPost]
        [Route("fleet.repo.game.RepoSVC/GetVersion")]
        public string LauncherGetVersion([FromBody] LauncherVersionRequest? body)
        {
            if (body == null)
            {
                return "{}";
            }
            
            return System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gameversion.json"));
        }

        public class LauncherVersionRequest
        {
            public int game_id {get;set;}
            public int branch_id { get; set; }
        }
    }
}
