using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InvoluteGears;
using Plotter;
using GearClient.Shared;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GearClient.Server.Controllers
{
    [ApiController]
    public class GearsController : ControllerBase
    {
        [HttpPost("api/involute")]
        public GearProfiles CalcInvoluteImage(GearParams gParams)
        {
            GearParameters gear = new GearParameters(
                gParams.Teeth,
                gParams.Module / 100.0,
                Math.PI * gParams.PressureAngle / 1800.0,
                gParams.ProfileShift / 1000.0,
                gParams.Tolerance / 100.0,
                gParams.Backlash / (double)gParams.Module,
                gParams.CutterDiameter / 100.0);

            Cutouts cutoutCalculator = new Cutouts(
                gear,
                gParams.SpindleDiameter / 100.0,
                gParams.InlayDiameter / 100.0,
                gParams.KeyFlatWidth / 100.0);

            // Create the output plot file of the gear

            //List<IEnumerable<PointF>> gearPoints = new List<IEnumerable<PointF>>
            //{
            //    Involutes.CirclePoints(-Math.PI, Math.PI, Involutes.AngleStep, gear.PitchCircleDiameter / 2),
            //    Involutes.CirclePoints(-Math.PI, Math.PI, Involutes.AngleStep, gear.BaseCircleDiameter / 2),
            //    Involutes.CirclePoints(-Math.PI , Math.PI, Involutes.AngleStep, gear.AddendumCircleDiameter / 2),
            //    Involutes.CirclePoints(-Math.PI, Math.PI, Involutes.AngleStep, gear.DedendumCircleDiameter / 2)
            //};

            List<IEnumerable<PointF>> gearPoints = new List<IEnumerable<PointF>>();

            for (int i = 0; i < gear.ToothCount; i++)
                gearPoints.AddRange(gear.GeneratePointsForOnePitch(i));
            GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

            // Now convert to image bytes to return from Web API

            using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048, Color.Black);
            using MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            ms.Seek(0L, SeekOrigin.Begin);
            Byte[] bytes = ms.GetBuffer();

            return new GearProfiles
            {
                Description = cutoutCalculator.Gear.Information + cutoutCalculator.Information,
                JpegBase64 = Convert.ToBase64String(bytes),
                SvgData = GearGenerator.GenerateSVG(cutoutCalculator, (float)gear.AddendumCircleDiameter)
            };
        }
    }
}
