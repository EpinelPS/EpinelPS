using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/ProfileCard/Possession/Get")]
public class GetProfileCardPossession : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqProfileCardObjectList req = await ReadData<ReqProfileCardObjectList>();
        var user = GetUser();
        ResProfileCardObjectList response = new();

        var pcoTable = GameData.Instance.ProfileCardObjectTable.Values.ToList();
        var rareCards = pcoTable.Where(x => x.GradeType == ProfileCardObjectGradeType.R)
                .Select(c => c.Id).ToList();

        user.ProfileCardsData.AddRange(rareCards.Except(user.ProfileCardsData));
        user.ProfileCardsData.ForEach(c =>
        {
            GameData.Instance.ProfileCardObjectTable.TryGetValue(c, out var card);
            switch (card.ObjectType)
            {
                case ObjectType.BackGround:
                    response.BackgroundIds.Add(c);
                    break;
                case ObjectType.Sticker:
                    response.StickerIds.Add(c);
                    break;
                default:
                    break;
            }
        });

        await WriteDataAsync(response);
    }
}
