using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/ProfileCard/Possession/Get")]
public class GetProfileCardPossession : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqProfileCardObjectList>();

        ResProfileCardObjectList response = new();

        foreach (var card in GameData.Instance.ProfileCardObjectTable.Values)
        {
            switch (card.ObjectType)
            {
                case ObjectType.BackGround:
                    response.BackgroundIds.Add(card.Id);
                    break;
                case ObjectType.Sticker:
                    response.StickerIds.Add(card.Id);
                    break;
                default:
                    break;
            }
        }

        await WriteDataAsync(response);
    }
}
