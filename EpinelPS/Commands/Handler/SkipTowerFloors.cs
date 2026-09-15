using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler
{
    public class SkipTowerFloorsParameter : ICommandParameters
    {
        public static ParameterDescriptor[] Descriptors => [
            Param.Int(0, "tower", "Tower number"),
            Param.Int(1, "floor", "Floor number"),
        ];

        public int Tower { get; init; }
        public int Floor { get; init; }
    }

    public class SkipTowerFloorsHandler(IExecutionContext context) : BaseHandler<SkipTowerFloorsParameter>(context)
    {
        public override string Name => "skip-tower";
        public override string Description => "Skip tower floor for the selected user, towerid floor (Elysion=1, Misslis=2, Tetra=3, Pilgrim=4, Tribe=5) e.g 1 234";

        protected async override Task<HandleResult> ExecuteAsync(SkipTowerFloorsParameter parameters)
        {
            if (context.SelectedUser == null)
                return new HandleResult(false, "No user selected");

            var rsp = AdminCommands.SkipTowerFloor(context.SelectedUser.ID, $"{parameters.Tower}-{parameters.Floor}");
            return rsp.ok
                ? new HandleResult(true, $"Tower {(CorporationTowerType)parameters.Tower}-{parameters.Floor} completed successfully")
                : new HandleResult(false, rsp.error);
        }
    }
}