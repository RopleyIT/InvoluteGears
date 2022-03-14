using InvoluteGears;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TwoDimensionLib;
using CmdArgs;
using System.Linq;

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
        Console.WriteLine("  -c | --customratios comma-sep-pressure-angles comma-sep-tooth-counts module cutter-diameter");
        Console.WriteLine("    Creates a table of contact ratios for teeth of various pressure angles and tooth counts.");
        Console.WriteLine("    Cutter diameter and module are measured in 100ths of a millimeter.");
        Console.WriteLine("  -C filename");
        Console.WriteLine("    Same as -c option, but predefined standard pressure angles and tooth counts");
        Console.WriteLine("  -m numerator denominator minteeth maxteeth");
        Console.WriteLine("    Identifies gear pairs that would achieve the desired gear ratio numerator/denominator with");
        Console.WriteLine("    wheels having tooth counts in the range minteeth to maxteeth.");
    }

    private static void CreateGearPlot(Cutouts cutoutCalculator, double size)
    {
        // Create the output plot file of the gear

        List<IEnumerable<PointF>> gearPoints = new()
        {
            cutoutCalculator.Gear.GenerateCompleteGearPath().FromCoords()
        };
        GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

        // Now convert to image bytes to return from Web API

        using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048, Color.Black);
        using FileStream ms = new(cutoutCalculator.Gear.ShortName + ".jpg", 
            FileMode.Create, FileAccess.Write, FileShare.None);
        img.Save(ms, ImageFormat.Jpeg);

        GearGenerator.GenerateSVGFile(cutoutCalculator, (float)size, cutoutCalculator.Gear.ShortName + ".svg");
    }

    private static void PlotInvolute(CommonArgs common, CutOutArgs cutOut, InvoluteArgs involute)
    {
        InvoluteGearParameters gear = new(
            common.Teeth,
            common.Module,
            Math.PI * involute.PressureAngle / 180.0,
            involute.ProfileShift / 100.0,
            common.Tolerance,
            common.Backlash / common.Module,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new (gear, 
            cutOut.SpindleDiameter, cutOut.InlayDiameter, 
            cutOut.KeyFlatWidth);

        CreateGearPlot(cutoutCalculator, gear.AddendumCircleDiameter);
    }

    private static void PlotCycloid(CommonArgs common, CutOutArgs cutOut, CycloidArgs cycloid)
    {
        CycloidalGear gear = new(
            common.Teeth,
            cycloid.OpposingTeeth,
            cycloid.ToothBlunting/100.0,
            cycloid.OpposingToothBlunting/100.0,
            common.Module,
            common.Tolerance,
            common.Backlash / common.Module,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new (gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateGearPlot(cutoutCalculator, gear.AddendumDiameter);
    }

    private static void PlotChain(CommonArgs common, CutOutArgs cutOut, ChainArgs chain)
    {
        ChainSprocket gear = new(
            common.Teeth,
            chain.WireThickness,
            common.Tolerance,
            chain.InnerLinkLength,
            chain.OuterLinkWidth,
            common.CutterDiameter,
            common.Backlash);

        Cutouts cutoutCalculator = new (gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);
        cutoutCalculator.AddPlot
            (gear.GenerateInnerGearPath().ToList());

        CreateGearPlot(cutoutCalculator,
            gear.InnerDiameter + 2 * gear.OuterLinkWidth);
    }

    private static void PlotEscape(CommonArgs common, CutOutArgs cutOut, EscapeArgs escape)
    {
        EscapeGearParameters gear = new(
            common.Teeth,
            common.Module,
            Math.PI * escape.UndercutAngle / 180.0,
            escape.FaceLength,
            escape.TipPitch,
            common.CutterDiameter,
            common.Tolerance);

        Cutouts cutoutCalculator = new (gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter);
    }

    private static void PlotRatchet(CommonArgs common, CutOutArgs cutOut, RatchetArgs ratchet)
    {
        Ratchet gear = new(
            common.Teeth,
            common.Module,
            common.Tolerance,
            ratchet.InnerDiameter,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new (gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateGearPlot(cutoutCalculator, gear.PitchCircleDiameter);
    }

    private static void PlotRoller(CommonArgs common, CutOutArgs cutOut, RollerSprocketArgs roller)
    {
        RollerSprocket gear = new(
            common.Teeth,
            roller.Pitch,
            common.Tolerance,
            roller.RollerDiameter,
            common.Backlash,
            roller.ChainWidth,
            common.CutterDiameter);

        Cutouts cutoutCalculator = new (gear,
            cutOut.SpindleDiameter, cutOut.InlayDiameter,
            cutOut.KeyFlatWidth);

        CreateGearPlot(cutoutCalculator, gear.OuterDiameter);
    }

    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Usage("gears [cycloid|chain|escape|involute|ratchet|roller|-m|-C|-c] [arguments]");
            return;
        }

        CommonArgs common = new ();
        CutOutArgs cutOut = new ();
        InvoluteArgs involute = new ();
        ChainArgs chain = new ();
        CycloidArgs cycloid = new ();
        EscapeArgs escape = new ();
        RatchetArgs ratchet = new ();
        RollerSprocketArgs roller = new ();

        Span<string> opts = new (args, 1, args.Length - 1);
        switch(args[0])
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
            case "-c":
            case "-customratios":
                if (args.Length != 6)
                {
                    Usage("-c pressure,angles tooth,counts module cutterdiameter");
                    return;
                }
                string[] values = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries);

                List<int> angles = new();
                foreach (string s in values)
                    if (int.TryParse(s, out int result))
                        angles.Add(result);
                    else
                    {
                        Usage("Pressure angles should be a comma-separated list of integers, measured in tenths of a degree");
                        return;
                    }

                values = args[3].Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<int> toothList = new();
                foreach (string s in values)
                    if (int.TryParse(s, out int result))
                        toothList.Add(result);
                    else
                    {
                        Usage("Tooth counts should be a comma-separated list of integers");
                        return;
                    }

                if (!int.TryParse(args[4], out int module))
                {
                    Usage("Module should be an integer measured in 100ths of a mm");
                    return;
                }

                if (!int.TryParse(args[5], out int cutterDiameter))
                {
                    Usage("Cutter diameter should be an integer measured in 100ths of a mm");
                    return;
                }

                using (StreamWriter sw = new(args[1]))
                    sw.Write(GenerateGearTables(angles, toothList, module, cutterDiameter));
                return;
            default:
                Usage("gears [cycloid|chain|escape|involute|ratchet|roller|-m|-C|-c] [arguments]");
                return;
        }

        if (args.Length > 0)
        {
            if (args[0] == "-c")
            {
                if (args.Length != 6)
                {
                    Usage("-c option needs 5 arguments");
                    return;
                }
                string[] values = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries);

                List<int> angles = new();
                foreach (string s in values)
                    if (int.TryParse(s, out int result))
                        angles.Add(result);
                    else
                    {
                        Usage("Pressure angles should be a comma-separated list of integers, measured in tenths of a degree");
                        return;
                    }

                values = args[3].Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<int> toothList = new();
                foreach (string s in values)
                    if (int.TryParse(s, out int result))
                        toothList.Add(result);
                    else
                    {
                        Usage("Tooth counts should be a comma-separated list of integers");
                        return;
                    }

                if (!int.TryParse(args[4], out int module))
                {
                    Usage("Module should be an integer measured in 100ths of a mm");
                    return;
                }

                if (!int.TryParse(args[5], out int cutterDiameter))
                {
                    Usage("Cutter diameter should be an integer measured in 100ths of a mm");
                    return;
                }

                using StreamWriter sw = new(args[1]);
                sw.Write(GenerateGearTables(angles, toothList, module, cutterDiameter));
                return;
            }
        }
    }

    public static string GenerateGearTables(IList<int> angles, IList<int> teeth, int module, int cutterDiameter)
    {
        using StringWriter sw = new();
        foreach (int pa in angles)
        {
            sw.WriteLine($"PRESSURE ANGLE {pa / 10.0:F1} DEGREES");
            sw.WriteLine($"MODULE {module / 100.0:F2}mm, CUTTER DIAMETER {cutterDiameter / 100.0:F2}mm");
            for (double s = 0; s <= 0.211; s += 0.03)
            {
                sw.WriteLine($"CONTACT RATIO FOR PROFILE SHIFTS {s * 100}% + {s * 100}%");
                sw.Write("TEETH");
                foreach (int t in teeth)
                    sw.Write($"{t,3}     ");
                sw.WriteLine();

                sw.Write("GAP ");
                List<InvoluteGearParameters> gears = new();
                foreach (int i in teeth)
                {
                    InvoluteGearParameters gear = new(i, module / 100.0, Math.PI * pa / 1800.0, s, 0, 0, cutterDiameter / 100.0);
                    sw.Write($"{gear.ToothGapAtUndercut,7:F3} ");
                    gears.Add(gear);
                }
                sw.WriteLine();

                sw.Write("Db  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.BaseCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Dd  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.DedendumCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Dc  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.InnerDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Du  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.UndercutRadius * 2,7:F3} ");
                sw.WriteLine();

                sw.Write("Dp  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.PitchCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Da  ");
                foreach (InvoluteGearParameters gear in gears)
                    sw.Write($"{gear.AddendumCircleDiameter,7:F3} ");
                sw.WriteLine();

                for (int i = 0; i < teeth.Count; i++)
                {
                    sw.Write($"{teeth[i],3} ");
                    for (int j = 0; j < teeth.Count; j++)
                        if (j < i)
                            sw.Write(".       ");
                        else
                            sw.Write($"{gears[i].ContactRatioWith(gears[j]),7:F3} ");
                    sw.WriteLine();
                }
                sw.WriteLine();
            }
        }
        return sw.ToString();
    }
}
