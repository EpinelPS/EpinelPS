using Google.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    public class GetNoticeContent : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            WriteJsonString("{\r\n  \"msg\": \"success\",\r\n  \"notice_list\": [\r\n    {\r\n      \"app_id\": \"3001001\",\r\n      \"app_notice_id\": \"post-6rpvwgrdx1b\",\r\n      \"area_list\": \"[\\\"81\\\",\\\"82\\\",\\\"83\\\",\\\"84\\\",\\\"85\\\"]\",\r\n      \"content_list\": [\r\n        {\r\n          \"app_content_id\": \"post-9ilpu79xxzp\",\r\n          \"content\": \"This isn't working\",\r\n          \"extra_data\": \"{}\",\r\n          \"id\": 48706,\r\n          \"lang_type\": \"en\",\r\n          \"picture_list\": [\r\n            {\r\n              \"extra_data\": \"{\\\"id\\\":\\\"TitleImage\\\"}\",\r\n              \"hash\": \"44a99a61152b5b80a0466ff9f0cee2bc\",\r\n              \"redirect_url\": \"\",\r\n              \"url\": \"pnt-console-cdn.playernetwork.intlgame.com/prod/29080/notice/022681b1121a40259a575fbe587651b4.jpg\"\r\n            }\r\n          ],\r\n          \"title\": \"New Character\",\r\n          \"update_time\": 1717637493\r\n        }\r\n      ],\r\n      \"end_time\": 1719431999,\r\n      \"extra_data\": \"{\\\"NoticeType\\\":\\\"Event\\\",\\\"Order\\\":\\\"11\\\",\\\"extra_reserved\\\":\\\"{\\\\\\\"Author\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"Category\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"CreateType\\\\\\\":\\\\\\\"4\\\\\\\",\\\\\\\"IsOpenService\\\\\\\":\\\\\\\"0\\\\\\\",\\\\\\\"IsToping\\\\\\\":false,\\\\\\\"Keyword\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"Sort\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"TopEnd\\\\\\\":\\\\\\\"2000-01-01 00:00:01\\\\\\\",\\\\\\\"TopStart\\\\\\\":\\\\\\\"2000-01-01 00:00:01\\\\\\\"}\\\"}\",\r\n      \"id\": 7560,\r\n      \"picture_list\": [],\r\n      \"start_time\": 1717617599,\r\n      \"status\": 1,\r\n      \"update_time\": 1717637494\r\n    }\r\n  ],\r\n  \"ret\": 0,\r\n  \"seq\": \"" + Seq + "\"\r\n}");
        }
    }
}
