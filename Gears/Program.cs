﻿using CmdArgs;
using InvoluteGears;
using Plotter;
using System;
using System.Collections.Generic;
using System.IO;
using TwoDimensionLib;

namespace Gears;

internal class Program
{
    private static void Usage(string info)
    {
        if (!string.IsNullOrWhiteSpace(info))
            Console.WriteLine(info);
        Console.Write(Arguments.Describe(new CommonArgs(), new CutOutArgs(),
            new InvoluteArgs(), new ChainArgs(), new CycloidArgs(),
            new EscapeArgs(), new RatchetArgs(), new RollerSprocketArgs()));
        Console.WriteLine("Utility options");
        Console.WriteLine("  -c | --customratios pressure-angle teeth1,teeth2 module shift1 shift2 cutterdiameter filename");
        Console.WriteLine("    Creates a table of contact ratios for the selected pressure angle and tooth counts.");
        Console.WriteLine("    The two profile shifts shift1 and shift2 are applied to each pair of tooth counts.");
        Console.WriteLine("    Cutter diameter, profile shifts and module are measured in 100ths of a millimeter.");
        Console.WriteLine("    The pressure angle is specified in tenths of a degree.");
        Console.WriteLine("  -C filename");
        Console.WriteLine("    Same as -c option, but predefined standard pressure angles and tooth counts");
        Console.WriteLine("  -m numerator denominator minteeth maxteeth");
        Console.WriteLine("    Identifies gear pairs that would achieve the desired gear ratio numerator/denominator with");
        Console.WriteLine("    wheels having tooth counts in the range minteeth to maxteeth.");
    }

    private static void CreateSvgGearPlot(Cutouts cutoutCalculator)
    {
        // Check for errors that might prevent the output SVG
        // file from being usable for CNC machining

        string errors = cutoutCalculator.Gear.Errors + cutoutCalculator.Errors;
        if (!string.IsNullOrWhiteSpace(errors))
            Usage(errors);
        else
        {
            // Find the square that contains the whole plot

            Rectangle bounds = cutoutCalculator.Curves.Bounds;
            double size = Math.Max(bounds.Width, bounds.Height);
            GearGenerator.GenerateSVGFile(cutoutCalculator,
                (float)size, cutoutCalculator.Gear.ShortName);
        }
    }

    private static void PlotInvolute(CommonArgs common, CutOutArgs cutOut, InvoluteArgs involute)
    {
        InvoluteGearParameters gear = new(
            common.Teeth,
            common.Module,
            Math.PI * involute.PressureAngle / 180.0,
            involute.ProfileShift / 100.0,
            common.Backlash / common.Module,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void PlotCycloid(CommonArgs common, CutOutArgs cutOut, CycloidArgs cycloid)
    {
        CycloidalGear gear = new(
            common.Teeth,
            cycloid.OpposingTeeth,
            cycloid.ToothBlunting / 100.0,
            cycloid.OpposingToothBlunting / 100.0,
            common.Module,
            common.Backlash / common.Module,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void PlotChain(CommonArgs common, CutOutArgs cutOut, ChainArgs chain)
    {
        ChainSprocket gear = new(
            common.Teeth,
            chain.WireThickness,
            chain.InnerLinkLength,
            chain.OuterLinkWidth,
            common.CutterDiameter,
            common.Backlash);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void PlotEscape(CommonArgs common, CutOutArgs cutOut, EscapeArgs escape)
    {
        EscapeGearParameters gear = new(
            common.Teeth,
            common.Module,
            Math.PI * escape.UndercutAngle / 180.0,
            escape.FaceLength,
            escape.TipPitch,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void PlotRatchet(CommonArgs common, CutOutArgs cutOut, RatchetArgs ratchet)
    {
        Ratchet gear = new(
            common.Teeth,
            common.Module,
            ratchet.InnerDiameter,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void PlotRoller(CommonArgs common, CutOutArgs cutOut, RollerSprocketArgs roller)
    {
        RollerSprocket gear = new(
            common.Teeth,
            roller.Pitch,
            roller.RollerDiameter,
            common.Backlash,
            roller.ChainWidth,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new(gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateSvgGearPlot(cutoutCalculator);
    }

    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Usage("gears [cycloid|chain|escape|involute|ratchet|roller|-m|-C|-c] [arguments]");
            return;
        }

        CommonArgs common = new();
        CutOutArgs cutOut = new();
        InvoluteArgs involute = new();
        ChainArgs chain = new();
        CycloidArgs cycloid = new();
        EscapeArgs escape = new();
        RatchetArgs ratchet = new();
        RollerSprocketArgs roller = new();

        Span<string> opts = new(args, 1, args.Length - 1);
        switch (args[0])
        {
            case "involute":
                Arguments.Parse(opts, common, cutOut, involute);
                PlotInvolute(common, cutOut, involute);
                break;
            case "cycloid":
                Arguments.Parse(opts, common, cutOut, cycloid);
                PlotCycloid(common, cutOut, cycloid);
                break;
            case "chain":
                Arguments.Parse(opts, common, cutOut, chain);
                PlotChain(common, cutOut, chain);
                break;
            case "escape":
                Arguments.Parse(opts, common, cutOut, escape);
                PlotEscape(common, cutOut, escape);
                break;
            case "ratchet":
                Arguments.Parse(opts, common, cutOut, ratchet);
                PlotRatchet(common, cutOut, ratchet);
                break;
            case "roller":
                Arguments.Parse(opts, common, cutOut, roller);
                PlotRoller(common, cutOut, roller);
                break;
            case "-m":
            case "--matchedpairs":
                if (args.Length != 5 ||
                    !int.TryParse(args[1], out int numerator) || !int.TryParse(args[2], out int denominator)
                    || !int.TryParse(args[3], out int minTeeth) | !int.TryParse(args[4], out int maxTeeth))
                {
                    Usage("gears -m numerator denominator minteeth maxteeth");
                    return;
                }
                Console.WriteLine(InvoluteGearParameters.MatchedPairs(numerator, denominator, minTeeth, maxTeeth));
                return;
            case "-C":
            case "--contactratios":
                if (args.Length != 2)
                {
                    Usage("-C option needs just a filename as an argument");
                    return;
                }
                int[] pressureAngles = new int[] { 145, 200, 250 };
                int[] teeth = new int[] { 8, 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 };
                using (StreamWriter sw = new(args[1]))
                    sw.Write(GenerateGearTables(pressureAngles, teeth, 100, 0));
                return;

            case "--bez":
                Arguments.Parse(opts, common, involute);
                DoBezierDemo(common.Module, common.Teeth,
                    involute.PressureAngle, involute.ProfileShift, common.CutterDiameter);
                return;

            case "-c":
            case "--customratios":
                if (args.Length != 8)
                {
                    Usage("-c pressure-angle teeth1,teeth2 module shift1 shift2 cutterdiameter filename");
                    return;
                }

                if (!int.TryParse(args[1], out int angle))
                {
                    Usage("Pressure angle should be an integer, measured in tenths of a degree");
                    return;
                }

                string[] values = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<int> toothList = new();
                foreach (string s in values)
                    if (int.TryParse(s, out int result))
                        toothList.Add(result);
                    else
                    {
                        Usage("Tooth counts should be a comma-separated list of integers");
                        return;
                    }

                if (!int.TryParse(args[3], out int module))
                {
                    Usage("Module should be an integer measured in 100ths of a mm");
                    return;
                }

                if (!int.TryParse(args[4], out int shift1)
                    || !int.TryParse(args[5], out int shift2))
                {
                    Usage("Profile shifts should be a percentage of the module");
                    return;
                }

                if (!int.TryParse(args[6], out int cutterDiameter))
                {
                    Usage("Cutter diameter should be an integer measured in 100ths of a mm");
                    return;
                }

                using (StreamWriter sw = new(args[7]))
                    GenerateGearTable(sw, angle, toothList, module, cutterDiameter, shift1 / 100.0, shift2 / 100.0);
                return;
            default:
                Usage("gears [cycloid|chain|escape|involute|ratchet|roller|-m|-C|-c] [arguments]");
                return;
        }
    }

    private static void DoBezierDemo(double module, int teeth, double pressureAngle, double profileShift, double offX)
    {
        // Adjust units

        pressureAngle = Geometry.DegToRad(pressureAngle);
        profileShift /= 100;

        var pitchRadius = module * teeth / 2.0;
        var pitchToBase = teeth * (1 - Math.Cos(pressureAngle)) / 2;
        if (pitchToBase <= 1 - profileShift)
            Console.WriteLine("dedendum extends beneath base circle. Adjusting...");

        // Now create the Bezier control points

        var baseCircleRadius = module * teeth / 2.0 * Math.Cos(pressureAngle);
        Coordinate bezFunc(double v) => new(
                baseCircleRadius * (Math.Cos(v) + v * Math.Sin(v)) + offX * Math.Cos(v),
                baseCircleRadius * (Math.Sin(v) - v * Math.Cos(v)) + offX * Math.Sin(v));
        double dedendumRadius = Math.Max
            (pitchRadius + (profileShift - 1) * module, baseCircleRadius);
        double addendumRadius = pitchRadius + (profileShift + 1) * module;
        //double endAngle = Geometry
        //    .RootDiffOfSquares(addendumRadius, baseCircleRadius) / baseCircleRadius;
        double endAngle = Math.PI / 4;
        double startAngle = Geometry
            .RootDiffOfSquares(dedendumRadius, baseCircleRadius) / baseCircleRadius;

        // First create the sequence of points as per standard line segment involutes

        List<Coordinate> lineSegs = new();
        for (double phi = 0; phi < Math.PI / 4; phi += Geometry.AngleStep)
            lineSegs.Add(Geometry.InvolutePlusOffset
                (module * (teeth / 2.0 - pitchToBase), offX, 0, phi, 0));

        if (startAngle <= 0)
            startAngle += (endAngle - startAngle) * 0.01; // Avoid the pole
        double midAngle = startAngle + 0.25 * (endAngle - startAngle);
        Spline involuteSpline = new(3, bezFunc, startAngle, midAngle);
        var coordsInner = involuteSpline.ControlPoints;

        involuteSpline = new Spline(3, bezFunc, midAngle, endAngle);
        var coordsOuter = involuteSpline.ControlPoints;

        // Now plot a graph

        SVGCreator svg = new();
        svg.AddPath(lineSegs, false, "black", 0.1, "transparent");
        SVGPath bez = new();
        bez.MoveTo(coordsInner[0]);
        bez.Cubic(coordsInner[1].X, coordsInner[1].Y, coordsInner[2].X, coordsInner[2].Y,
            coordsInner[3].X, coordsInner[3].Y);
        svg.AddPath(bez, "green", 0.1, "transparent");
        bez = new SVGPath();
        bez.MoveTo(coordsOuter[0]);
        bez.Cubic(coordsOuter[1].X, coordsOuter[1].Y, coordsOuter[2].X, coordsOuter[2].Y,
            coordsOuter[3].X, coordsOuter[3].Y);
        svg.AddPath(bez, "red", 0.1, "transparent");

        svg.CalculateViewBox(new Coordinate(2, 2));
        svg.HasXmlHeader = true;

        // Write out the plot

        using StreamWriter sw = new("bezier.svg");
        sw.WriteLine(svg.ToString());
        sw.Close();
    }

    private static void RenderLine(TextWriter sw, string header,
        IEnumerable<InvoluteGearParameters> gears,
        Func<InvoluteGearParameters, string> selector)
    {
        sw.Write(header);
        foreach (var g in gears)
            sw.Write(selector(g));
        sw.WriteLine();
    }

    public static void GenerateGearTable(TextWriter sw, int pa, IList<int> teeth,
        int module, int cutterDiameter, double shift1, double shift2)
    {
        // Build the two tables of involute gears, one for each profile shift percentage

        List<InvoluteGearParameters> gShift1 = new();
        List<InvoluteGearParameters> gShift2 = new();
        foreach (int i in teeth)
        {
            InvoluteGearParameters gear = new(i, module / 100.0, Math.PI * pa / 1800.0, shift1, 0, cutterDiameter / 100.0);
            gShift1.Add(gear);
            gear = new(i, module / 100.0, Math.PI * pa / 1800.0, shift2, 0, cutterDiameter / 100.0);
            gShift2.Add(gear);
        }

        sw.Write($"PRESSURE ANGLE {pa / 10.0:F1}°");
        sw.WriteLine($" MODULE {module / 100.0:F2}mm, CUTTER DIAMETER {cutterDiameter / 100.0:F2}mm");
        sw.WriteLine($"GEAR DATA & CONTACT RATIOS FOR PROFILE SHIFTS {shift1 * 100}% (ROWS) + {shift2 * 100}% (COLUMNS)");
        sw.Write("TEETH   ");
        foreach (int t in teeth)
            sw.Write($"     {t,3}");
        sw.WriteLine();

        RenderLine(sw, $"G  {shift1 * 100,3}% ", gShift1, g => $" {g.ToothGapAtUndercut,7:F3}");
        RenderLine(sw, $"G  {shift2 * 100,3}% ", gShift2, g => $" {g.ToothGapAtUndercut,7:F3}");
        RenderLine(sw, $"Db      ", gShift1, g => $" {g.BaseCircleDiameter,7:F3}");
        RenderLine(sw, $"Dd {shift1 * 100,3}% ", gShift1, g => $" {g.DedendumCircleDiameter,7:F3}");
        RenderLine(sw, $"Dd {shift2 * 100,3}% ", gShift2, g => $" {g.DedendumCircleDiameter,7:F3}");
        RenderLine(sw, $"Dc {shift1 * 100,3}% ", gShift1, g => $" {g.InnerDiameter,7:F3}");
        RenderLine(sw, $"Dc {shift2 * 100,3}% ", gShift2, g => $" {g.InnerDiameter,7:F3}");
        RenderLine(sw, $"Du {shift1 * 100,3}% ", gShift1, g => $" {g.UndercutRadius,7:F3}");
        RenderLine(sw, $"Du {shift2 * 100,3}% ", gShift2, g => $" {g.UndercutRadius,7:F3}");
        RenderLine(sw, $"Dp      ", gShift1, g => $" {g.PitchCircleDiameter,7:F3}");
        RenderLine(sw, $"Da {shift1 * 100,3}% ", gShift1, g => $" {g.AddendumCircleDiameter,7:F3}");
        RenderLine(sw, $"Da {shift2 * 100,3}% ", gShift2, g => $" {g.AddendumCircleDiameter,7:F3}");

        for (int i = 0; i < teeth.Count; i++)
        {
            sw.Write($"{teeth[i],3}     ");
            for (int j = 0; j < teeth.Count; j++)
                sw.Write($" {gShift1[i].ContactRatioWith(gShift2[j]),7:F3}");
            sw.WriteLine();
        }
        sw.WriteLine();
    }

    public static string GenerateGearTables(IList<int> angles, IList<int> teeth, int module, int cutterDiameter)
    {
        using StringWriter sw = new();
        foreach (int pa in angles)
            for (double s = 0; s <= 0.211; s += 0.03)
                GenerateGearTable(sw, pa, teeth, module, cutterDiameter, s, -s);
        return sw.ToString();
    }
}
