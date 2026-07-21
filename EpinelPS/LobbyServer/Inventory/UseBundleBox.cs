using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/usebundlebox")]
public class UseBundleBox : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUseBundleBox req = await ReadData<ReqUseBundleBox>();
        User user = GetUser();

        ResUseBundleBox response = new();

        DbItemData bundleBox = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault()
            ?? throw new InvalidDataException("cannot find bundleBox with isn " + req.Isn);

        var totalBundleBoxes = req.Count;
        if (totalBundleBoxes > bundleBox.Count)
            throw new Exception("count mismatch");

        bundleBox.Count -= totalBundleBoxes;
        if (bundleBox.Count == 0) user.Items.Remove(bundleBox);

        response.Reward = NetUtils.UseBundleBox(user, bundleBox.ItemType, req.Count);
        response.Reward.UserItems.Add(NetUtils.UserItemDataToNet(bundleBox));

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
