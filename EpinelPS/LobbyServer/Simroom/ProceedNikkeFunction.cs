using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/proceednikkefunction")]
    public class ProceedNikkeFunction : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "location": { "chapter": 3, "stage": 6, "order": 1 }, "event": 10040, "selectionNumber": 1, "selectionGroupElementId": 100401 }
            ReqProceedSimRoomNikkeFunction req = await ReadData<ReqProceedSimRoomNikkeFunction>();
            User user = GetUser();

            ResProceedSimRoomNikkeFunction response = new()
            {
                Result = SimRoomResult.Success
            };

            var location = req.Location;
            // Check
            var events = user.ResetableData.SimRoomData.Events;
            var simRoomEventIndex = events.FindIndex(x => x.Location.Chapter == location.Chapter && x.Location.Stage == location.Stage && x.Location.Order == location.Order);
            if (simRoomEventIndex < 0)
            {
                Logging.Warn("Not Fond UserSimRoomEvent");
                await WriteDataAsync(response);
            }

            // changedHps
            try
            {
                if (GameData.Instance.SimulationRoomSelectionGroupTable.TryGetValue(req.SelectionGroupElementId, out var selectionGroup))
                {
                    if (selectionGroup.EventFunctionType == SimulationRoomEventFunctionType.Heal)
                    {
                        if (selectionGroup.EventFunctionTargetType == SimulationRoomEventfunctionTargetType.All)
                        {
                            var changedRemainingHps = UpdateUserRemainingHps(user, selectionGroup.EventFunctionValue, type: SimulationRoomEventFunctionType.Heal);
                            response.ChangedHps.AddRange(changedRemainingHps.Select(x => new NetSimRoomCharacterHp { Csn = x.Csn, Hp = x.Hp }));

                            SimRoomHelper.UpdateUserSimRoomEvent(user, index: simRoomEventIndex, events, selectionNumber: req.SelectionNumber, isDone: true);
                        }
                        else
                        {
                            Logging.Warn($"Not implement EventFunctionTargetType: {selectionGroup.EventFunctionTargetType}");
                        }
                    }
                    else if (selectionGroup.EventFunctionType == SimulationRoomEventFunctionType.Resurrection)
                    {
                        if (selectionGroup.EventFunctionTargetType == SimulationRoomEventfunctionTargetType.All)
                        {
                            var changedRemainingHps = UpdateUserRemainingHps(user, selectionGroup.EventFunctionValue, type: SimulationRoomEventFunctionType.Resurrection);
                            response.ChangedHps.AddRange(changedRemainingHps.Select(x => new NetSimRoomCharacterHp { Csn = x.Csn, Hp = x.Hp }));

                            SimRoomHelper.UpdateUserSimRoomEvent(user, index: simRoomEventIndex, events, selectionNumber: req.SelectionNumber, isDone: true);
                        }
                        else
                        {
                            Logging.Warn($"Not implement EventFunctionTargetType: {selectionGroup.EventFunctionTargetType}");
                        }
                    }
                    else
                    {
                        Logging.Warn($"Not implement EventFunctionType: {selectionGroup.EventFunctionType}");
                    }

                }
                else
                {
                    Logging.Warn("Not Fond SimulationRoomSelectionGroup");
                }
            }
            catch (Exception e)
            {
                Logging.Warn($"ProceedNikkeFunction ChangedHps Exception {e.Message}");
            }

            // Teams
            var team = SimRoomHelper.GetTeamData(user, 1, null);
            if (team is not null) response.Teams.Add(team);

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        public static List<SimRoomCharacterHp> UpdateUserRemainingHps(User user, int HpValue, SimulationRoomEventFunctionType type = SimulationRoomEventFunctionType.Heal)
        {
            var remainingHps = user.ResetableData.SimRoomData.RemainingHps;
            if (remainingHps is not null && remainingHps.Count > 0)
            {
                for (int i = 0; i < remainingHps.Count; i++)
                {
                    if (type is SimulationRoomEventFunctionType.Heal && remainingHps[i].Hp >= 0)
                    {
                        // Heal
                        remainingHps[i] = new SimRoomCharacterHp { Csn = remainingHps[i].Csn, Hp = HpValue };
                    }
                    else if (type is SimulationRoomEventFunctionType.Resurrection && remainingHps[i].Hp < 0)
                    {
                        // Resurrection
                        remainingHps[i] = new SimRoomCharacterHp { Csn = remainingHps[i].Csn, Hp = HpValue };
                    }
                }
                user.ResetableData.SimRoomData.RemainingHps = remainingHps;
                return remainingHps;
            }
            return remainingHps;
        }

    }
}