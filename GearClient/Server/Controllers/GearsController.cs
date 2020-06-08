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

            return CreateGearPlot(cutoutCalculator, gear.AddendumCircleDiameter);
        }

        [HttpPost("api/invzip")]
        public async Task<IActionResult> CalcInvoluteSvgZip(GearParams gParams)
        {
            var profiles = CalcInvoluteImage(gParams);
            var zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return File(zipStream, "application/zip");
        }

        private GearProfiles CreateGearPlot(Cutouts cutoutCalculator, double size)
        {
            // Create the output plot file of the gear

            var gearPoints = new List<IEnumerable<PointF>>();
            gearPoints.Add(cutoutCalculator.Gear.GenerateCompleteGearPath());
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
                ShortName = cutoutCalculator.Gear.ShortName,
                JpegBase64 = Convert.ToBase64String(bytes),
                SvgData = GearGenerator.GenerateSVG(cutoutCalculator, (float)size)
            };
        }

        [HttpPost("api/escape")]
        public GearProfiles CalcEscapeImage(EscapeWheelParams gParams)
        {
            EscapeGearParameters gear = new EscapeGearParameters(
                gParams.Teeth,
                double.Parse(gParams.Module),
                Math.PI * double.Parse(gParams.UndercutAngle) / 180.0,
                double.Parse(gParams.FaceLength),
                double.Parse(gParams.TipPitch),
                double.Parse(gParams.BaseDiameter),
                double.Parse(gParams.Tolerance));

            Cutouts cutoutCalculator = new Cutouts(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

            return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter);
        }

        [HttpPost("api/esczip")]
        public async Task<IActionResult> CalcEscapeWheelSvgZip(EscapeWheelParams gParams)
        {
            var profiles = CalcEscapeImage(gParams);
            var zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return File(zipStream, "application/zip");
        }
    }
}
