namespace EpinelPS
{
    public class ChannelInfo
    {
        public string openid { get; set; } = "";
        public string token { get; set; } = "";
        public int account_type { get; set; }
        public string account { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public int account_plat_type { get; set; }
        public string lang_type { get; set; } = "";
        public bool is_login { get; set; }
        public string account_uid { get; set; } = "";
        public string account_token { get; set; } = "";
    }
    public class AuthPkt
    {
        public ChannelInfo channel_info { get; set; } = new();
    }
    public class AuthPkt2
    {
        public string token { get; set; } = "";
        public string openid { get; set; } = "";
        public string account_token { get; set; } = "";
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
    public class LoginEndpoint2Req
    {
        public DeviceInfo device_info { get; set; } = new();
        public string extra_json { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string password { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public int support_captcha { get; set; }
    }

    public class RegisterEPReq
    {
        public DeviceInfo device_info { get; set; } = new();
        public string verify_code { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string phone_area_code { get; set; } = "";
        public string password { get; set; } = "";
        public string user_name { get; set; } = "";
        public string birthday { get; set; } = "";
        public string region { get; set; } = "";
        public string user_lang_type { get; set; } = "";
        public string extra_json { get; set; } = "";
    }
    public class SendCodeRequest
    {
        public DeviceInfo device_info { get; set; } = new();
        public string extra_json { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string phone_area_code { get; set; } = "";
        public int code_type { get; set; }
        public int support_captcha { get; set; }
    }
}
