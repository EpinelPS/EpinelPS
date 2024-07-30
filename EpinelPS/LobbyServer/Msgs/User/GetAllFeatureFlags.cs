using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/featureflags/all/get")]
    public class GetAllFeatureFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAllFeatureFlags>();

            var response = new ResGetAllFeatureFlags();
            response.Flags.AddRange([
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.UnionRaid2, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.CooperationEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.RookieArena, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.SimulationRoom, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.DailyEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Intercept, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Attendance, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Dice, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.StoryDungeonEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Tower, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.LostSector2, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Subscription2, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ArchiveEventSystem, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ViewShortCut, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.PollEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ComeBackUserEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.EventPass, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.FieldHubEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ShopEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.MissionEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ChargeGachaEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.MiniGameSortOut, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.CharacterSkillResetEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.EventQuest, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.RewardUpEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.SdbattleEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.TextAdventure, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.ChallengeModeEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.DailyFreeGachaEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.BoxGachaEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.DiceEvent, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Bbqtycoon, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.Ce002MiniGame, IsOpen = true },
                    new NetFeatureFlag() { FeatureKind = NetFeatureKind.SoloRaid, IsOpen = true },
                ]);
            await WriteDataAsync(response);
        }
    }
}
