namespace EpinelPS.LobbyServer.Character.Counsel;

[GameRequest("/character/counsel/answer/list")]
public class ListAnswered : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListAnsweredAttractiveCounsel req = await ReadData<ReqListAnsweredAttractiveCounsel>();
        User user = GetUser();

        ResListAnsweredAttractiveCounsel response = new();

        NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);
        if (bondInfo != null)
        {
            response.CorrectlyAnsweredAttractiveCounselTableIdList.AddRange(bondInfo.CounselDialogCompleteIds);
        }

        await WriteDataAsync(response);
    }
}
