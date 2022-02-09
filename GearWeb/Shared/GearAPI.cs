using InvoluteGears;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace GearWeb.Shared
{
    public static class GearAPI
    {
        public static GearProfiles CalcInvoluteImage(GearParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            GearParameters gear = new(
                gParams.Teeth,
                double.Parse(gParams.Module),
                Math.PI * double.Parse(gParams.PressureAngle) / 180.0,
                double.Parse(gParams.ProfileShift) / 100.0,
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.Backlash) / double.Parse(gParams.Module),
                double.Parse(gParams.CutterDiameter));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

            return CreateGearPlot(cutoutCalculator, gear.AddendumCircleDiameter);
        }

        public static Stream CalcInvoluteSvgZip(GearParams gParams)
        {
            GearProfiles profiles = CalcInvoluteImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        private static GearProfiles CreateGearPlot(Cutouts cutoutCalculator, double size)
        {
            // Create the output plot file of the gear

            List<IEnumerable<PointF>> gearPoints = new()
            {
                cutoutCalculator.Gear.GenerateCompleteGearPath().FromCoords()
            };
            GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

            // Now convert to image bytes to return from Web API

            using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048, Color.Black);
            using MemoryStream ms = new();
            img.Save(ms, ImageFormat.Jpeg);
            ms.Seek(0L, SeekOrigin.Begin);
            byte[] bytes = ms.GetBuffer();

            return new GearProfiles
            {
                Description = cutoutCalculator.Gear.Information + cutoutCalculator.Information,
                ShortName = cutoutCalculator.Gear.ShortName,
                JpegBase64 = Convert.ToBase64String(bytes),
                SvgData = GearGenerator.GenerateSVG(cutoutCalculator, (float)size)
            };
        }

        public static GearProfiles CalcEscapeImage(EscapeWheelParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            EscapeGearParameters gear = new(
                gParams.Teeth,
                double.Parse(gParams.Module),
                Math.PI * double.Parse(gParams.UndercutAngle) / 180.0,
                double.Parse(gParams.FaceLength),
                double.Parse(gParams.TipPitch),
                double.Parse(gParams.BaseDiameter),
                double.Parse(gParams.Tolerance));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

            return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter);
        }

        public static Stream CalcEscapeWheelSvgZip(EscapeWheelParams gParams)
        {
            GearProfiles profiles = CalcEscapeImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        public static GearProfiles CalcRatchetImage(RatchetParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            Ratchet gear = new(
                gParams.Teeth,
                double.Parse(gParams.Module),
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.InnerDiameter),
                double.Parse(gParams.CutterDiameter));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

            return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter);
        }

        public static Stream CalcRatchetSvgZip(RatchetParams gParams)
        {
            GearProfiles profiles = CalcRatchetImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        public static GearProfiles CalcChainSprocketImage(ChainSprocketParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            ChainSprocket gear = new(
                gParams.Teeth,
                double.Parse(gParams.WireThickness),
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.InnerLinkLength),
                double.Parse(gParams.OuterLinkWidth),
                double.Parse(gParams.CutterDiameter),
                double.Parse(gParams.Backlash));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));
            cutoutCalculator.AddPlot
                (gear.GenerateInnerGearPath().ToList());

            return CreateGearPlot(cutoutCalculator,
                gear.InnerDiameter + 2 * gear.OuterLinkWidth);
        }

        public static Stream CalcChainSProcketSvgZip(ChainSprocketParams gParams)
        {
            GearProfiles profiles = CalcChainSprocketImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        public static GearProfiles CalcRollerSprocketImage(RollerSprocketParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            RollerSprocket gear = new(
                gParams.Teeth,
                double.Parse(gParams.Pitch),
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.RollerDiameter),
                double.Parse(gParams.Backlash),
                double.Parse(gParams.ChainWidth),
                double.Parse(gParams.CutterDiameter));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));
            cutoutCalculator.AddPlot
                (gear.GenerateInnerGearPath().ToList());

            return CreateGearPlot(cutoutCalculator, gear.OuterDiameter);
        }

        public static Stream CalcRollerSProcketSvgZip(RollerSprocketParams gParams)
        {
            GearProfiles profiles = CalcRollerSprocketImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }
    }
}
