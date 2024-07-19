namespace nksrv.IntlServer
{
    public class IntlReturnJsonHandler : IntlMsgHandler
    {
        private string JsonToReturn;
        public override bool RequiresAuth => false;

        public IntlReturnJsonHandler(string jsonToReturn)
        {
            JsonToReturn = jsonToReturn;
        }

        protected override async Task HandleAsync()
        {
            await WriteJsonStringAsync(JsonToReturn.Replace("((SEGID))", Seq));
        }
    }
}
