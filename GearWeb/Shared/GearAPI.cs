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
            Cutouts cutoutCalculator = BuildGear
                (gParams.Teeth, gParams.ProfileShift, gParams);
            Cutouts opposingCutoutCalculator = BuildGear
                (gParams.OpposingTeeth, gParams.OpposingProfileShift, gParams);

            switch(gParams.WhichGears)
            {
                case 1: // Left or primary gear
                    return CreateGearPlot(cutoutCalculator, true);
                case 2: // Right or opposing gear
                    return CreateGearPlot(opposingCutoutCalculator, true);
                case 0: // Meshing
                    return CreateSequencedGearPlots
                        (cutoutCalculator, opposingCutoutCalculator, true);
                case 3: // Both full gears
                    return CreateSequencedGearPlots
                        (cutoutCalculator, opposingCutoutCalculator, false);
                default:
                    return CreateGearPlot(cutoutCalculator, true); // Temporarily
            }
        }

        private static Cutouts BuildGear(int teeth, string profileShift, GearParams gp)
        {
            if (gp == null)
                throw new ArgumentNullException(nameof(gp));

            InvoluteGearParameters gear = new(
                teeth,
                double.Parse(gp.Module),
                Math.PI * double.Parse(gp.PressureAngle) / 180.0,
                double.Parse(profileShift) / 100.0,
                double.Parse(gp.Tolerance),
                double.Parse(gp.Backlash) / double.Parse(gp.Module),
                double.Parse(gp.CutterDiameter));
            
            Cutouts cutoutCalculator = new(
                gear,
                double.Parse(gp.SpindleDiameter),
                double.Parse(gp.InlayDiameter),
                double.Parse(gp.KeyFlatWidth));
            
            if (gp.ShowCircles)
            {
                //var circle = cutoutCalculator.CalcCircle(gear.PitchCircleDiameter / 2);
                //cutoutCalculator.AddPlot(circle, "blue", "transparent");
                cutoutCalculator.InsertCurve
                    (Cutouts.CircularCurve(gear.PitchCircleDiameter / 2), "blue", "transparent");
                //circle = cutoutCalculator.CalcCircle(gear.BaseCircleDiameter / 2);
                //cutoutCalculator.AddPlot(circle, "green", "transparent");
                cutoutCalculator.InsertCurve
                    (Cutouts.CircularCurve(gear.BaseCircleDiameter / 2), "green", "transparent");
                //circle = cutoutCalculator.CalcCircle(gear.AddendumCircleDiameter / 2);
                //cutoutCalculator.AddPlot(circle, "red", "transparent");
                cutoutCalculator.InsertCurve
                   (Cutouts.CircularCurve(gear.AddendumCircleDiameter / 2), "red", "transparent");
                //circle = cutoutCalculator.CalcCircle(gear.DedendumCircleDiameter / 2);
                //cutoutCalculator.AddPlot(circle, "magenta", "transparent");
                cutoutCalculator.InsertCurve
                   (Cutouts.CircularCurve(gear.DedendumCircleDiameter / 2), "goldenrod", "transparent");
            }
            return cutoutCalculator;
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

            return CreateGearPlot(cutoutCalculator, false);
            //return CreateGearPlot(cutoutCalculator, gear.AddendumDiameter, false);
        }

        public static Stream CalcInvoluteSvgZip(GearParams gParams)
        {
            GearProfiles profiles = CalcInvoluteImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }

        /// <summary>
        /// Assuming the two gears are involute gears, compute their contact ratio
        /// </summary>
        /// <param name="gear">The left gear</param>
        /// <param name="oppositeGear">the right gear</param>
        /// <returns>A string describing the contact ratio</returns>
        
        private static string ContactRatioInfo(IGearProfile gear, IGearProfile oppositeGear)
        {
            if (gear is InvoluteGearParameters
                    && oppositeGear is InvoluteGearParameters)
            {
                (double contactRatio, double centreDistance) = (gear as InvoluteGearParameters)
                    .ContactRatioWith(oppositeGear as InvoluteGearParameters);
                return $"Contact ratio: {contactRatio:N2}\r\nDistance between centres: {centreDistance:N2}\r\n";
            }
            return string.Empty;
        }

        /// <summary>
        /// When the plotting type is set to Both or Meshing (3 or 0 respectively)
        /// create the sequence of images of both gears as their teeth rotate while
        /// intermeshing.
        /// </summary>
        /// <param name="cutout">The cout out of the left or primary gear</param>
        /// <param name="opposingCutout">The cutout of the right or opposite gear</param>
        /// <param name="zoom">If true, show just the teeth in the meshing area
        /// zoomed to fill the whole image sequence. If false, show both teeth
        /// meshing in their entirety.</param>
        /// <returns>A gear description profile containing the image sequences
        /// and a textual description for them</returns>
        
        private static GearProfiles CreateSequencedGearPlots
            (Cutouts cutout, Cutouts opposingCutout, bool zoom)
        {
            var profiles = new GearProfiles
            {
                Description = "LEFT GEAR\r\n"
                    + cutout.Gear.Information + cutout.Information
                    + "RIGHT GEAR\r\n"
                    + opposingCutout.Gear.Information
                    + ContactRatioInfo(cutout.Gear, opposingCutout.Gear),
                ShortName = cutout.Gear.ShortName+"_"+opposingCutout.Gear.ShortName,
                SvgPlot = new string[] { string.Empty },
                SvgData = string.Empty,
                Errors = cutout.Gear.Errors + cutout.Errors
                    + opposingCutout.Gear.Errors + opposingCutout.Errors
            };

            if (string.IsNullOrWhiteSpace(profiles.Errors))
            {
                List<string> strokes = new List<string>(cutout.StrokeColours);
                List<string> fills = new List<string>(cutout.FillColours);

                // Find the square that contains the whole plot

                Rectangle bounds = cutout.Curves.Bounds;
                double size = Math.Max(bounds.Width, bounds.Height);

                // Retrieve the the two wheels and offset them to the right position.
                // This places the origin on the common X axis between the two
                // gear centres, and offset by their Pitch Radii. The gears are also
                // rotated to a reference angle where the two gears are toucing at their
                // respective pitch circle radii.

                DrawableSet leftGear = cutout.Curves;
                DrawableSet rightGear = opposingCutout.Curves;
                if ((opposingCutout.Gear.ToothCount & 1) == 0)
                    rightGear = rightGear.RotatedBy
                        (Math.PI / opposingCutout.Gear.ToothCount, Coordinate.Empty) as DrawableSet;
                
                // Plot 2^N diagrams of gears at 2^N angles covering one full tooth meshing
                // Find the step size for the left gear then the right gear

                int N = 6;
                double leftStep = 2 * Math.PI / ((1<<N) * cutout.Gear.ToothCount);
                double rightStep = 2 * Math.PI / ((1<<N) * opposingCutout.Gear.ToothCount);
                profiles.SvgPlot = new string[1 << N];
                for(int i = 0; i < (1<<N); i++)
                {
                    var leftDrawables = leftGear.RotatedBy(leftStep * i, Coordinate.Empty)
                       .Translated(new Coordinate(-cutout.Gear.PitchRadius, 0)) as DrawableSet;
                    var rightDrawables = rightGear.RotatedBy(-rightStep * i, Coordinate.Empty)
                       .Translated(new Coordinate(opposingCutout.Gear.PitchRadius, 0)) as DrawableSet;
                    var meshedDrawables = leftDrawables.Merge(rightDrawables);
                    Rectangle plotBounds;
                    if (zoom)
                    {
                        double m = 2 * Math.PI * cutout.Gear.Module;
                        plotBounds = new Rectangle
                            (new Coordinate(-m, -m), 2 * m, 2 * m);
                    }
                    else
                        plotBounds = meshedDrawables.Bounds;
                    profiles.SvgPlot[i] = SVGPlot.PlotCurves
                        (meshedDrawables, 640, 640, plotBounds, strokes, fills);
                }
                profiles.SvgData = GearGenerator
                    .GenerateSVGCurves(cutout, (float)size);
            }
            return profiles;
        }

        private static GearProfiles CreateGearPlot(Cutouts cutoutCalculator, bool usePaths)
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
                SvgPlot = new string[] { string.Empty },
                SvgData = string.Empty,
                Errors = cutoutCalculator.Gear.Errors + cutoutCalculator.Errors
            };

            if (string.IsNullOrWhiteSpace(profiles.Errors))
            {
                List<string> strokes = new List<string>();
                List<string> fills = new List<string>();
                if(!usePaths)
                {
                    strokes.Add("black");
                    fills.Add("transparent");
                }
                strokes.AddRange(cutoutCalculator.StrokeColours);
                fills.AddRange(cutoutCalculator.FillColours);

                // Find the square that contains the whole plot

                Rectangle bounds = cutoutCalculator.Curves.Bounds;
                double size = Math.Max(bounds.Width, bounds.Height);
                
                if (usePaths)
                {
                    DrawableSet dPaths = new DrawableSet
                    {
                        Paths = new List<DrawablePath>
                            (cutoutCalculator.Curves.Paths)
                    };
                    profiles.SvgPlot = new string[]
                    {
                        SVGPlot.PlotCurves
                            (dPaths, 640, 640, strokes, fills)
                    };
                    profiles.SvgData = GearGenerator
                        .GenerateSVGCurves(cutoutCalculator, (float)size);
                }
                else
                {
                    profiles.SvgPlot = new string[]
                    {
                        SVGPlot.PlotGraphs(gearPoints, 640, 640, strokes, fills)
                    };
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

            return CreateGearPlot(cutoutCalculator, false);
            //return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter, false);
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

            return CreateGearPlot(cutoutCalculator, false);
            //return CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter, false);
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

            return CreateGearPlot(cutoutCalculator, false);
            //return CreateGearPlot(cutoutCalculator,
            //    gear.InnerDiameter + 2 * gear.OuterLinkWidth, false);
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

            return CreateGearPlot(cutoutCalculator, false);
            //return CreateGearPlot(cutoutCalculator, gear.OuterDiameter, false);
        }

        public static Stream CalcRollerSProcketSvgZip(RollerSprocketParams gParams)
        {
            GearProfiles profiles = CalcRollerSprocketImage(gParams);
            Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
            return zipStream;
        }
    }
}
