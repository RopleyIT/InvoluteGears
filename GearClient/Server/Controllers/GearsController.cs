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
                double.Parse(gParams.Module),
                Math.PI * double.Parse(gParams.PressureAngle) / 180.0,
                double.Parse(gParams.ProfileShift) / 100.0,
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.Backlash) / double.Parse(gParams.Module),
                double.Parse(gParams.CutterDiameter));

            Cutouts cutoutCalculator = new Cutouts(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

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
