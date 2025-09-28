using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/building")]
    public class BuildBuilding : LobbyMsgHandler
    {
        private static BuildingCost GetBuildingCost(int buildingId)
        {
            // TODO: get building cost from  data
            // test data
            return buildingId switch
            {
                // bulIdding (22xxx)
                22401 => new BuildingCost { Gold = 1000, BuildTimeMinutes = 5 },
                22701 => new BuildingCost { Gold = 2000, BuildTimeMinutes = 10 },
                22801 => new BuildingCost { Gold = 1500, BuildTimeMinutes = 8 },
                22901 => new BuildingCost { Gold = 3000, BuildTimeMinutes = 15 },
                23001 => new BuildingCost { Gold = 5000, BuildTimeMinutes = 20 },
                23100 => new BuildingCost { Gold = 800, BuildTimeMinutes = 6 },
                23200 => new BuildingCost { Gold = 1200, BuildTimeMinutes = 8 },
                23300 => new BuildingCost { Gold = 2500, BuildTimeMinutes = 12 },
                23400 => new BuildingCost { Gold = 2000, BuildTimeMinutes = 10 },
                23500 => new BuildingCost { Gold = 1800, BuildTimeMinutes = 9 },

                // bulIdding (10xxx-12xxx)
                10100 => new BuildingCost { Gold = 500, BuildTimeMinutes = 3 },
                10200 => new BuildingCost { Gold = 800, BuildTimeMinutes = 5 },
                10300 => new BuildingCost { Gold = 1200, BuildTimeMinutes = 7 },
                10400 => new BuildingCost { Gold = 900, BuildTimeMinutes = 6 },
                10500 => new BuildingCost { Gold = 700, BuildTimeMinutes = 4 },
                10600 => new BuildingCost { Gold = 1500, BuildTimeMinutes = 8 },
                10700 => new BuildingCost { Gold = 1100, BuildTimeMinutes = 7 },
                10800 => new BuildingCost { Gold = 1300, BuildTimeMinutes = 8 },
                10900 => new BuildingCost { Gold = 1600, BuildTimeMinutes = 9 },
                11000 => new BuildingCost { Gold = 1000, BuildTimeMinutes = 6 },
                11100 => new BuildingCost { Gold = 1400, BuildTimeMinutes = 8 },
                11200 => new BuildingCost { Gold = 1800, BuildTimeMinutes = 10 },
                11300 => new BuildingCost { Gold = 800, BuildTimeMinutes = 5 },
                11400 => new BuildingCost { Gold = 2000, BuildTimeMinutes = 11 },
                11500 => new BuildingCost { Gold = 1200, BuildTimeMinutes = 7 },
                11600 => new BuildingCost { Gold = 1700, BuildTimeMinutes = 9 },
                11700 => new BuildingCost { Gold = 1300, BuildTimeMinutes = 8 },
                11800 => new BuildingCost { Gold = 900, BuildTimeMinutes = 6 },
                11900 => new BuildingCost { Gold = 2200, BuildTimeMinutes = 12 },
                12000 => new BuildingCost { Gold = 1500, BuildTimeMinutes = 9 },
                12100 => new BuildingCost { Gold = 3000, BuildTimeMinutes = 15 },
                12200 => new BuildingCost { Gold = 2800, BuildTimeMinutes = 14 },
                12300 => new BuildingCost { Gold = 1600, BuildTimeMinutes = 9 },

                // default building cost
                _ => new BuildingCost { Gold = 1000, BuildTimeMinutes = 5 }
            };
        }

        protected override async Task HandleAsync()
        {
            ReqBuilding req = await ReadData<ReqBuilding>();
            User user = GetUser();
            
       
            BuildingCost cost = GetBuildingCost(req.BuildingId);

            if (!user.CanSubtractCurrency(CurrencyType.Gold, cost.Gold))
            {
                
                ResBuilding errorResponse = new()
                {
                    StartAt = 0,
                    CompleteAt = 0
                };
                await WriteDataAsync(errorResponse);
                return;
            }

            bool goldDeducted = user.SubtractCurrency(CurrencyType.Gold, cost.Gold);
            if (!goldDeducted)
            {
                ResBuilding errorResponse = new()
                {
                    StartAt = 0,
                    CompleteAt = 0
                };
                await WriteDataAsync(errorResponse);
                return;
            }


            DateTime  startTime = DateTime.UtcNow;
            DateTime  completeTime = startTime.AddMinutes(cost.BuildTimeMinutes);
            
            NetUserOutpostData newBuilding = new NetUserOutpostData()
            {
                SlotId = req.PositionId,
                BuildingId = req.BuildingId,
                IsDone = false,
                StartAt = startTime.Ticks,
                CompleteAt = completeTime.Ticks
            };

            bool found = false;
            for (int i = 0; i < user.OutpostBuildings.Count; i++)
            {
                if (user.OutpostBuildings[i].SlotId == req.PositionId)
                {
                    user.OutpostBuildings[i] = newBuilding;
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                user.OutpostBuildings.Add(newBuilding);
            }

            JsonDb.Save();

            ResBuilding response = new()
            {
                StartAt = newBuilding.StartAt,
                CompleteAt = newBuilding.CompleteAt
            };
            
            await WriteDataAsync(response);
        }
    }

    public class BuildingCost
    {
        public long Gold { get; set; } = 0;
        public int BuildTimeMinutes { get; set; } = 5;
        // public long Materials { get; set; } = 0;
        // public long SpecialCurrency { get; set; } = 0;
    }
}
