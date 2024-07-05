using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    internal class AutoLoginEndpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            await WriteJsonStringAsync("{\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + Seq + "\\\"}\",\"del_account_status\":0,\"del_li_account_status\":0,\"extra_json\":{\"del_li_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + Seq + "\\\"}\",\"get_status_msg\":\"success\",\"get_status_ret\":0,\"get_status_rsp\":{\"adult_age\":14,\"adult_age_map\":{},\"adult_check_status\":1,\"adult_check_status_expiration\":\"0\",\"adult_status_map\":{},\"certificate_type\":3,\"email\":\"\",\"eu_user_agree_status\":0,\"game_grade\":0,\"game_grade_map\":{},\"is_dma\":true,\"is_eea\":false,\"is_need_li_cert\":false,\"msg\":\"success\",\"need_parent_control\":0,\"need_realname_auth\":0,\"parent_certificate_status\":0,\"parent_certificate_status_expiration\":\"0\",\"parent_control_map\":{},\"qr_code_ret\":0,\"realname_auth_status\":0,\"region\":\"724\",\"ret\":0,\"ts\":\"" + DateTimeOffset.UtcNow.ToUnixTimeSeconds()
+ "\"},\"need_notify_msg\":\"success\",\"need_notify_ret\":0,\"need_notify_rsp\":{\"has_bind_li\":true,\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"user_agreed_game_dma\":\"2\",\"user_agreed_game_pp\":\"1\",\"user_agreed_game_tos\":\"1\",\"user_agreed_li_dt\":\"\",\"user_agreed_li_pp\":\"1\",\"user_agreed_li_tos\":\"\"}},\"msg\":\"success\",\"ret\":0,\"seq\":\"" + Seq + "\"}");
        }
    }
}
