using InvoluteGears;
using Plotter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace GearWeb.Shared
{
    public static class GearAPI
    {
        public static GearProfiles CalcInvoluteImage(GearParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            InvoluteGearParameters gear = new(
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
            
            if(gParams.ShowCircles)
            {
                var circle = cutoutCalculator.CalcCircle(gear.PitchCircleDiameter / 2);
                cutoutCalculator.AddPlot(circle, "blue", "transparent");
                circle = cutoutCalculator.CalcCircle(gear.BaseCircleDiameter / 2);
                cutoutCalculator.AddPlot(circle, "green", "transparent");
                circle = cutoutCalculator.CalcCircle(gear.AddendumCircleDiameter / 2);
                cutoutCalculator.AddPlot(circle, "red", "transparent");
                circle = cutoutCalculator.CalcCircle(gear.DedendumCircleDiameter / 2);
                cutoutCalculator.AddPlot(circle, "magenta", "transparent");
            }

            return CreateGearPlot(cutoutCalculator, gear.AddendumCircleDiameter, true);
        }

        public static GearProfiles CalcCycloidImage(CycloidParams gParams)
        {
            if (gParams == null)
                throw new ArgumentNullException(nameof(gParams));

            CycloidalGear gear = new(
                gParams.Teeth,
                gParams.OpposingTeeth,
                gParams.ToothBlunting / 100.0,
                gParams.OpposingToothBlunting / 100.0,
                double.Parse(gParams.Module),
                double.Parse(gParams.Tolerance),
                double.Parse(gParams.Backlash) / double.Parse(gParams.Module),
                double.Parse(gParams.CutterDiameter));

            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gParams.SpindleDiameter),
                double.Parse(gParams.InlayDiameter),
                double.Parse(gParams.KeyFlatWidth));

            return CreateGearPlot(cutoutCalculator, gear.AddendumDiameter, false);
        }

        public static Stream CalcInvoluteSvgZip(GearParams gParams)
        {
            GearProfiles profiles = CalcInvoluteImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        private static GearProfiles CreateGearPlot(Cutouts cutoutCalculator, double size, bool usePaths)
        {
            // Create the output plot file of the gear

            List<IEnumerable<Coordinate>> gearPoints = new()
            {
                cutoutCalculator.Gear.GenerateCompleteGearPath()
            };
            GearGenerator.AddCutoutsAndSpindles(cutoutCalculator, gearPoints);

            // Now convert to image bytes to return from Web API

            var profiles = new GearProfiles
            {
                Description = cutoutCalculator.Gear.Information + cutoutCalculator.Information,
                ShortName = cutoutCalculator.Gear.ShortName,
                SvgPlot = string.Empty,
                SvgData = string.Empty,
                Errors = cutoutCalculator.Gear.Errors + cutoutCalculator.Errors
            };

            if (string.IsNullOrWhiteSpace(profiles.Errors))
            {
                List<string> strokes = new List<string> { "black" };
                List<string> fills = new List<string> { "transparent" };
                strokes.AddRange(cutoutCalculator.StrokeColours);
                fills.AddRange(cutoutCalculator.FillColours);
                if (usePaths)
                {
                    IList<DrawablePath> dPaths = new List<DrawablePath>();
                    dPaths.Add(cutoutCalculator.Gear.GenerateGearCurve());
                    dPaths.AddRange(cutoutCalculator.Curves);
                    profiles.SvgPlot = SVGPlot.PlotCurves
                        (dPaths, 640, 640, strokes, fills);
                    profiles.SvgData = GearGenerator
                        .GenerateSVGCurves(cutoutCalculator, (float)size);
                }
                else
                {
                    profiles.SvgPlot = SVGPlot.PlotGraphs(gearPoints, 640, 640, strokes, fills);
                    profiles.SvgData = GearGenerator.GenerateSVG(cutoutCalculator, (float)size);
                }
            }
            return profiles;
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

            return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter, false);
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

            return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter, false);
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
                gear.InnerDiameter + 2 * gear.OuterLinkWidth, false);
        }

        public static Stream CalcChainSprocketSvgZip(ChainSprocketParams gParams)
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

            return CreateGearPlot(cutoutCalculator, gear.OuterDiameter, false);
        }

        public static Stream CalcRollerSProcketSvgZip(RollerSprocketParams gParams)
        {
            GearProfiles profiles = CalcRollerSprocketImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }
    }
}
