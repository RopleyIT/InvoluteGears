using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using InvoluteGears;
using Plotter;

namespace InvoluteGrpcServer
{
    public class GearsServer : GearService.GearServiceBase
    {
        public override Task<GearResponse> GenerateGearOutline
            (GearRequest request, ServerCallContext context)
        {
            GearParameters gear = new GearParameters(
                request.ToothCount, 
                request.Module, // In millimetres
                request.PressureAngle * Math.PI / 180, 
                request.ProfileShift / request.Module, // From millimetres to fractions of the module
                request.Tolerance, // In millimetres
                request.Backlash / request.Module // From millimetres to fractions of the module
                );
            return base.GenerateGearOutline(request, context);
        }
    }
}
