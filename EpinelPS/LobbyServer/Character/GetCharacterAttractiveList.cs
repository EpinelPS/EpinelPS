namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/attractive/get")]
public class GetCharacterAttractiveList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetAttractiveList req = await ReadData<ReqGetAttractiveList>();
        User user = GetUser();

        ResGetAttractiveList response = new()
        {
            CounselAvailableCount = 3 // TODO
        };

        foreach (var item in user.Characters)
        {
            if (item.RareType != Data.OriginalRareType.R && item.RareType != Data.OriginalRareType.None)
            {
                var data = new NetUserAttractiveData()
                {
                   NameCode = item.NameCode,
                   CanCounselToday = true, // TODO
                   CompleteRewardStatus = item.RewardStatus,
                   CounseledCount = item.TotalCounseledCount,
                   Lv = item.BondLevel,
                   Exp = item.BondLevelExp,
                   IsFavorites = item.Favorite,
                   
                };
                data.CounselDialogCompleteIds.AddRange(item.CompletedDialogs);
                data.FlushableWatchedDialogIds.AddRange(item.FlushableWatchedDialogIds);
                data.ObtainedRewardLevels.AddRange(item.ObtainedRewardLevels);
                response.Attractives.Add(data);
            }
        }

        await WriteDataAsync(response);
    }
}