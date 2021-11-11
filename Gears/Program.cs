using InvoluteGears;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace InvoluteConsole;

internal class Program
{
    private static void Main(string[] args)
    {
        // First chop the spindle description off the end of the arg list

        string[] spindleArgs = null;
        int idx = Array.FindIndex(args, a => string.Compare(a, "-s", true) == 0);
        if (idx >= 0)
        {
            spindleArgs = new string[args.Length - idx];
            Array.Copy(args, idx, spindleArgs, 0, spindleArgs.Length);
            string[] newArgs = new string[idx];
            Array.Copy(args, 0, newArgs, 0, idx);
            args = newArgs;
        }

        if (args.Length > 0)
        {
            if (args[0] == "-h")
            {
                Usage(null);
                return;
            }
            if (args[0] == "-m")
            {
                if (args.Length != 5 ||
                    !int.TryParse(args[1], out int numerator) || !int.TryParse(args[2], out int denominator)
                    || !int.TryParse(args[3], out int minTeeth) | !int.TryParse(args[4], out int maxTeeth))
                {
                    Usage("-m option needs 4 arguments");
                    return;
                }
                Console.WriteLine(GearParameters.MatchedPairs(numerator, denominator, minTeeth, maxTeeth));
                return;
            }
            if (args[0] == "-C")
            {
                if (args.Length != 2)
                {
                    Usage("-C option needs just a filename as an argument");
                    return;
                }
                int[] pressureAngles = new int[] { 145, 200, 250 };
                int[] teeth = new int[] { 8, 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 };
                using StreamWriter sw = new(args[1]);
                sw.Write(GenerateGearTables(pressureAngles, teeth, 100, 0));
                return;
            }
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
            if (args[0].ToLower() == "-p")
            {
                if (args.Length != 8
                    || !int.TryParse(args[1], out int teeth)
                    || !int.TryParse(args[2], out int shift)
                    || !int.TryParse(args[3], out int maxErr)
                    || !int.TryParse(args[4], out int pressureAngle)
                    || !int.TryParse(args[5], out int module)
                    || !int.TryParse(args[6], out int backlash)
                    || !int.TryParse(args[7], out int cutterDiameter))
                {
                    Usage("-p and -P options need seven arguments, plus an optional following -s argument list");
                    return;
                }
                GearParameters gear = new(
                    teeth,
                    module / 100.0,
                    Math.PI * pressureAngle / 1800.0,
                    shift / 1000.0,
                    maxErr / 100.0,
                    backlash / (double)module,
                    cutterDiameter / 100.0);

                Cutouts cutoutCalculator = CreateCutouts(gear, spindleArgs);

                // Generate the SVG version of the gear path

                GearGenerator.GenerateSVGFile(cutoutCalculator, (float)gear.AddendumCircleDiameter,
                    $"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}");

                // Create the output plot file of the gear

                int limit = gear.ToothCount;
                double angle = Math.PI;
                if (args[0] == "-P")
                {
                    limit = 1;
                    angle = gear.ToothAngle / 2;
                }

                List<IEnumerable<PointF>> gearPoints = new()
                {
                    Involutes.CirclePoints(-angle, angle, Involutes.AngleStep, gear.PitchCircleDiameter / 2),
                    Involutes.CirclePoints(-angle, angle, Involutes.AngleStep, gear.BaseCircleDiameter / 2)
                };

                //IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, GearParameters.AngleStep, gear.AddendumCircleDiameter / 2);
                //IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, GearParameters.AngleStep, gear.DedendumCircleDiameter / 2);

                for (int i = 0; i < limit; i++)
                    gearPoints.AddRange(gear.GeneratePointsForOnePitch(i));

                if (args[0] == "-p")
                    GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

                // Report what was created

                Console.WriteLine(gear.Information);
                Console.WriteLine(cutoutCalculator.Information);

                using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048);
                img.Save($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}.png", ImageFormat.Png);
            }
            if (args[0].ToLower() == "-e")
            {
                if (args.Length != 8
                    || !int.TryParse(args[1], out int teeth)
                    || !int.TryParse(args[2], out int maxErr)
                    || !int.TryParse(args[3], out int undercutAngle)
                    || !int.TryParse(args[4], out int module)
                    || !int.TryParse(args[5], out int toothFaceLength)
                    || !int.TryParse(args[6], out int tipPitch)
                    || !int.TryParse(args[7], out int cutDiameter))
                {
                    Usage("-e and -E options require 7 arguments, plus an optional -s argument list");
                    return;
                }
                EscapeGearParameters gear = new(
                    teeth,
                    module / 100.0,
                    Math.PI * undercutAngle / 1800.0,
                    toothFaceLength / 100.0,
                    tipPitch / 100.0,
                    cutDiameter / 100.0,
                    maxErr / 100.0
                    );

                Cutouts cutoutCalculator = CreateCutouts(gear, spindleArgs);

                // Generate the SVG version of the gear path

                GearGenerator.GenerateSVGFile(cutoutCalculator, (float)gear.PitchCircleDiameter,
                    $"e{gear.ToothCount}u{undercutAngle}m{module}f{toothFaceLength}p{tipPitch}d{cutDiameter}");

                // Create the output plot file of the gear

                List<IEnumerable<PointF>> gearPoints = new();
                int limit = gear.ToothCount;
                double angle = 2 * Math.PI;
                if (args[0] == "-E")
                {
                    limit = 1;
                    angle = gear.ToothAngle;

                    gearPoints.Add(Involutes
                        .CirclePoints(0, angle, Involutes.AngleStep, gear.PitchCircleDiameter / 2));
                    gearPoints.Add(Involutes
                        .CirclePoints(0, angle, Involutes.AngleStep, gear.InnerDiameter / 2));
                }

                for (int i = 0; i < limit; i++)
                    gearPoints.Add(gear.ToothProfile(i));

                if (args[0] == "-e")
                    GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

                // Report what was created

                Console.WriteLine(gear.Information);
                Console.WriteLine(cutoutCalculator.Information);

                using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048);
                img.Save($"e{gear.ToothCount}u{undercutAngle}m{module}f{toothFaceLength}p{tipPitch}d{cutDiameter}.png", ImageFormat.Png);
            }
            if (args[0].ToLower() == "-r")
            {
                if (args.Length != 6
                    || !int.TryParse(args[1], out int teeth)
                    || !int.TryParse(args[2], out int maxErr)
                    || !int.TryParse(args[3], out int module)
                    || !int.TryParse(args[4], out int innerDiameter)
                    || !int.TryParse(args[5], out int cutterrDiameter))
                {
                    Usage("-r option requires 5 arguments, plus an optional -s argument list");
                    return;
                }
                Ratchet gear = new(
                    teeth,
                    module / 100.0,
                    maxErr / 100.0,
                    innerDiameter / 100.0,
                    cutterrDiameter / 100.0
                    );

                Cutouts cutoutCalculator = CreateCutouts(gear, spindleArgs);

                // Generate the SVG version of the gear path

                GearGenerator.GenerateSVGFile(cutoutCalculator, (float)gear.PitchCircleDiameter,
                    $"t{gear.ToothCount}m{module}i{innerDiameter}");

                // Create the output plot file of the gear

                List<IEnumerable<PointF>> gearPoints = new();
                for (int i = 0; i < gear.ToothCount; i++)
                    gearPoints.Add(gear.ToothProfile(i));

                GearGenerator.GenerateCutoutPlot(cutoutCalculator, gearPoints);

                // Report what was created

                Console.WriteLine(gear.Information);
                Console.WriteLine(cutoutCalculator.Information);

                using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048);
                img.Save($"t{teeth}m{module}e{maxErr}i{innerDiameter}.png", ImageFormat.Png);
            }
        }
        else
            Usage("Missing or unrecognised arguments");
    }

    private static Cutouts CreateCutouts(IGearProfile gear, string[] spindleArgs)
    {
        if (spindleArgs == null
            || spindleArgs.Length != 4
            || !int.TryParse(spindleArgs[1], out int spindleDia)
            || !int.TryParse(spindleArgs[2], out int inlayDia)
            || !int.TryParse(spindleArgs[3], out int keyFlats))
        {
            Usage("-s option needs three arguments");
            return new Cutouts(gear, 0, 0, 0);
        }
        else return new Cutouts
            (gear, spindleDia / 100.0, inlayDia / 100.0, keyFlats / 100.0);
    }

    private static void Usage(string errMsg)
    {
        if (!string.IsNullOrEmpty(errMsg))
            Console.WriteLine($"ERROR: {errMsg}");
        else
            Console.WriteLine("Compute data or diagrams for involute gears and escape wheels.");
        Console.WriteLine("USAGE: gears -p|P|e|E|c|C|m gear-options [-s spindle-options]\r\n");
        Console.WriteLine(
            "-p|-P [tooth count] [profile shift] [tolerance] [angle] [module] [backlash] [cutter diameter]\r\n"
                + "\twhere -p generates whole gear image, -P one tooth image\r\n"
                + "\twhere tooth count is digits\r\n"
                + "\tprofile shift is in 10ths of a % of the module\r\n"
                + "\ttolerance is in 100ths of mm\r\n"
                + "\tangle is pressure angle in 10ths of a degree\r\n"
                + "\tmodule is in 100ths of a mm\r\n"
                + "\tbacklash is in 100ths of a mm\r\n"
                + "\tcutter diameter is in 100ths of a mm\r\n");
        Console.WriteLine("-e|-E [tooth count] [tolerance] [angle] [module] [tooth length] [tip pitch] [cut diameter]\r\n"
                + "\twhere -e generates whole escape wheel image, -E one tooth image\r\n"
                + "\twhere tooth count is digits\r\n"
                + "\ttolerance is in 100ths of mm\r\n"
                + "\tangle is undercut angle in 10ths of a degree\r\n"
                + "\tmodule is in 100ths of a mm\r\n"
                + "\ttooth length is in 100ths of a mm\r\n"
                + "\ttip pitch is in 100ths of a mm\r\n"
                + "\tcut diameter is in 100ths of a mm\r\n");
        Console.WriteLine("-r [tooth count] [tolerance] [module] [inner diameter] [cut diameter]\r\n"
                + "\twhere tooth count is digits\r\n"
                + "\ttolerance is in 100ths of mm\r\n"
                + "\tmodule is in 100ths of a mm\r\n"
                + "\tinner diameter is in 100ths of a mm\r\n"
                + "\tcut diameter is in 100ths of a mm\r\n");
        Console.WriteLine("-s [spindle] [inlay] [hex key], units all in 100ths mm");
        Console.WriteLine("\tOptionally -s can be used at the end of the -p, -P, -e or -E argument list");
        Console.WriteLine("\tspindle is centre bore diameter");
        Console.WriteLine("\tinlay is a larger diameter central bore for a bearing");
        Console.WriteLine("\thex key is the distance across flats for a hex key for attaching pinions to gears\r\n");
        Console.WriteLine("-m num denom teethMin teethMax -- find gear-pinion pairs with same separation");
        Console.WriteLine("\tnum: numerator of the overall gear ratio");
        Console.WriteLine("\tdenom: denominator of the overall gear ratio");
        Console.WriteLine("\tteethMin, teethMax: ranges of tooth counts for search\r\n");
        Console.WriteLine("-C output-file-path -- contact ratios for various profile shifts and angles");
        Console.WriteLine("\toutput-file-path: Where to store the results\r\n");
        Console.WriteLine("-c output-file-path comma-sep-angles comma-sep-teeth module cutter-diameter");
        Console.WriteLine("\toutput-file-path: Where to store the results");
        Console.WriteLine("\tcomma-sep-angles: pressure angles in 10ths of a degree");
        Console.WriteLine("\tcomma-sep-teeth: list of tooth counts");
        Console.WriteLine("\tmodule: gear module in 100ths of a mm");
        Console.WriteLine("\tcutter-diameter: diameter of end mill in 100ths of a mm\r\n");
        Console.WriteLine("-h -- generate this help text\r\n");
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
                List<GearParameters> gears = new();
                foreach (int i in teeth)
                {
                    GearParameters gear = new(i, module / 100.0, Math.PI * pa / 1800.0, s, 0, 0, cutterDiameter / 100.0);
                    sw.Write($"{gear.ToothGapAtUndercut,7:F3} ");
                    gears.Add(gear);
                }
                sw.WriteLine();

                sw.Write("Db  ");
                foreach (GearParameters gear in gears)
                    sw.Write($"{gear.BaseCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Dd  ");
                foreach (GearParameters gear in gears)
                    sw.Write($"{gear.DedendumCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Dc  ");
                foreach (GearParameters gear in gears)
                    sw.Write($"{gear.InnerDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Du  ");
                foreach (GearParameters gear in gears)
                    sw.Write($"{gear.UndercutRadius * 2,7:F3} ");
                sw.WriteLine();

                sw.Write("Dp  ");
                foreach (GearParameters gear in gears)
                    sw.Write($"{gear.PitchCircleDiameter,7:F3} ");
                sw.WriteLine();

                sw.Write("Da  ");
                foreach (GearParameters gear in gears)
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
