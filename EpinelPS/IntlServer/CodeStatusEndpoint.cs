namespace EpinelPS.IntlServer
{
    public class CodeStatusEndpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            // pretend that any code is valid
            await WriteJsonStringAsync("{\"expire_time\":759,\"msg\":\"Success\",\"ret\":0,\"seq\":\"" + Seq + "\"}");
        }
    }
}
