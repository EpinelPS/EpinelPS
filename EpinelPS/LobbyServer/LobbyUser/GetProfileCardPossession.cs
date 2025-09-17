using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/Possession/Get")]
    public class GetProfileCardPossession : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardObjectList req = await ReadData<ReqProfileCardObjectList>();
            User user = GetUser();

            ResProfileCardObjectList response = new();


            Dictionary<int, ProfileCardObjectTableRecord> ProfileCardObjects = GameData.Instance.ProfileCardObjectTable;
            List<ItemData> userfileCardObjects = [.. user.Items.Where(item =>
                ProfileCardObjects.ContainsKey(item.ItemType))];


            foreach (ItemData item in userfileCardObjects)
            {
                if (ProfileCardObjects.TryGetValue(item.ItemType, out ProfileCardObjectTableRecord ? ProfileCardObject)) {
                    if (ProfileCardObject.object_type.Equals("BackGround"))
                    {
                        response.BackgroundIds.Add(item.ItemType);
                    }
                    else if (ProfileCardObject.object_type.Equals("Sticker"))
                    {
                        response.StickerIds.Add(item.ItemType);
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
