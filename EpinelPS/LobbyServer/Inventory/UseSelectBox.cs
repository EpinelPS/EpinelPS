using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System.Text.Json;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/useselectbox")]
public class UseSelectBox : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUseSelectBox req = await ReadData<ReqUseSelectBox>();
        User user = GetUser();

        ResUseSelectBox response = new();

        DbItemData selectBox = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() 
            ?? throw new InvalidDataException("cannot find selectBox with isn " + req.Isn);

        var totalSelectBoxes = req.Select.Sum(x => x.Count);
        if (totalSelectBoxes > selectBox.Count)
            throw new Exception("count mismatch");

        selectBox.Count -= totalSelectBoxes;
        if (selectBox.Count == 0) user.Items.Remove(selectBox);

        response.Reward = NetUtils.UseSelectBox(user, selectBox.ItemType, req.Select);
        response.Reward.UserItems.Add(NetUtils.UserItemDataToNet(selectBox));

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
