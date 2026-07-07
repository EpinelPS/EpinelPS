using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/record/tutorial")]
public class RecordTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeRecordStellarBladeTutorial req = await ReadData<ReqArcadeRecordStellarBladeTutorial>();
        User user = GetUser();
        ResArcadeRecordStellarBladeTutorial response = new();

       if( user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId , out var stellar))
        {
            stellar.TutorialList.AddRangeUnique(req.TutorialListIds.ToList());            
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}