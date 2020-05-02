using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using InvoluteGears;
using Plotter;

namespace InvoluteConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-h")
                {
                    Console.WriteLine("Compute data or diagrams for involute gears. Options:");
                    Console.WriteLine(
                        "-p|-P [tooth count] [profile shift] [tolerance] [angle] [module] [backlash] [cutter diameter]\r\n"
                            + "\twhere -p generates whole gear image, -P one tooth image\r\n"
                            + "\twhere tooth count is digits\r\n"
                            + "\tprofile shift is in thousandths of the module\r\n"
                            + "\ttolerance is in 100ths of mm\r\n"
                            + "\tangle is pressure angle in 10ths of a degree\r\n"
                            + "\tmodule is in 100ths of a mm\r\n"
                            + "\tbacklash is in thousandths of the module\r\n"
                            + "\tcutter diameter is in 100ths of a mm");
                    Console.WriteLine("-m num denom teethMin teethMax -- find gear-pinion pairs with same separation");
                    Console.WriteLine("\tnum: numerator of the overall gear ratio");
                    Console.WriteLine("\tdenom: denominator of the overall gear ratio");
                    Console.WriteLine("\tteethMin, teethMax: ranges of tooth counts for search");
                    Console.WriteLine("-C output-file-path -- contact ratios for various profile shifts and angles");
                    Console.WriteLine("\toutput-file-path: Where to store the results");
                    Console.WriteLine("-c output-file-path comma-sep-angles comma-sep-teeth module cutter-diameter");
                    Console.WriteLine("\toutput-file-path: Where to store the results");
                    Console.WriteLine("\tcomma-sep-angles: pressure angles in 10ths of a degree");
                    Console.WriteLine("\tcomma-sep-teeth: list of tooth counts");
                    Console.WriteLine("\tmodule: gear module in 100ths of a mm");
                    Console.WriteLine("\tcutter-diameter: diameter of end mill in 100ths of a mm");
                    return;
                }
                if (args[0] == "-m")
                {
                    if (args.Length != 5 ||
                        !int.TryParse(args[1], out int numerator) || !int.TryParse(args[2], out int denominator)
                        || !int.TryParse(args[3], out int minTeeth) | !int.TryParse(args[4], out int maxTeeth))
                    {
                        Console.WriteLine("Usage: gears -m 13 5 10 108\r\n\tWhere 13/5 is the overall gear ratio, 10 and 108 the min and max number of teeth");
                        return;
                    }
                    Console.WriteLine(GearParameters.MatchedPairs(numerator, denominator, minTeeth, maxTeeth));
                    return;
                }
                if (args[0] == "-C")
                {
                    if (args.Length != 2)
                    {
                        Console.WriteLine("Usage: gears -C output-file-path -- contact ratios for various profile shifts and angles");
                        Console.WriteLine("\toutput-file-path: Where to store the results");
                        return;
                    }
                    int[] pressureAngles = new int[] { 145, 200, 250 };
                    int[] teeth = new int[] { 8, 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 };
                    using StreamWriter sw = new StreamWriter(args[1]);
                    sw.Write(GenerateGearTables(pressureAngles, teeth, 100, 0));
                    return;
                }
                if (args[0] == "-c")
                {
                    if (args.Length != 6)
                    {
                        Console.WriteLine("Usage: gears -c output-file-path comma-sep-angles comma-sep-teeth module cutter-diameter");
                        Console.WriteLine("\toutput-file-path: Where to store the results");
                        Console.WriteLine("\tcomma-sep-angles: pressure angles in 10ths of a degree");
                        Console.WriteLine("\tcomma-sep-teeth: list of tooth counts");
                        Console.WriteLine("\tmodule: Gear module in 100ths of a mm");
                        Console.WriteLine("\tcutter-diameter: Diameter of end mill in 100ths of a mm");
                        return;
                    }
                    string[] values = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries);

                    List<int> angles = new List<int>();
                    foreach (string s in values)
                        if (int.TryParse(s, out int result))
                            angles.Add(result);
                        else
                        {
                            Console.WriteLine("Angles should be a comma-separated list of integers, measured in tenths of a degree");
                            return;
                        }

                    values = args[3].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    List<int> toothList = new List<int>();
                    foreach (string s in values)
                        if (int.TryParse(s, out int result))
                            toothList.Add(result);
                        else
                        {
                            Console.WriteLine("Tooth counts should be a comma-separated list of integers");
                            return;
                        }

                    if (!int.TryParse(args[4], out int module))
                    {
                        Console.WriteLine("Module should be an integer measured in 100ths of a mm");
                        return;
                    }

                    if (!int.TryParse(args[5], out int cutterDiameter))
                    {
                        Console.WriteLine("Cutter diameter should be an integer measured in 100ths of a mm");
                        return;
                    }

                    using StreamWriter sw = new StreamWriter(args[1]);
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
                        Console.WriteLine("Usage: gears -p|-P [tooth count] [profile shift] [tolerance] [angle] [module] [backlash] [cutter diameter]\r\n"
                            + "\twhere -p generates whole gear image, -P one tooth image\r\n"
                            + "\twhere tooth count is digits\r\n"
                            + "\tprofile shift is in thousandths of the module\r\n"
                            + "\ttolerance is in 100ths of mm\r\n"
                            + "\tangle is pressure angle in 10ths of a degree\r\n"
                            + "\tmodule is in 100ths of a mm\r\n"
                            + "\tbacklash is in thousandths of the module\r\n"
                            + "\tcutter diameter is in 100ths of a mm");
                        return;
                    }
                    GearParameters gear = new GearParameters(
                        teeth,
                        module / 100.0,
                        Math.PI * pressureAngle / 1800.0,
                        shift / 1000.0,
                        maxErr / 100.0,
                        backlash / 1000.0,
                        cutterDiameter / 100.0);

                    Cutouts cutoutCalculator = new Cutouts(gear, 18.0, 8.0, 22.0, 7.0);
                    List<List<PointF>> cutouts = cutoutCalculator.CalculateCutouts();

                    // Generate the SVG version of the gear path

                    SVGPath svgPath = new SVGPath(gear.GenerateCompleteGearPath(), true);
                    SVGCreator svgCreator = new SVGCreator();
                    svgCreator.AddPath(svgPath);
                    if (cutouts != null)
                        foreach (List<PointF> cutout in cutouts)
                            svgCreator.AddPath(new SVGPath(cutout, true));
                    svgCreator.AddPath(new SVGPath(cutoutCalculator.CalculateInlay(), true));
                    svgCreator.AddPath(new SVGPath(cutoutCalculator.CalculateSpindle(), true));

                    //svgCreator.DocumentDimensions = new SizeF
                    //    ((float)(gear.Module * (gear.ToothCount + 2)), (float)(gear.Module * (gear.ToothCount + 2)));
                    //svgCreator.DocumentDimensionUnits = "mm";
                    svgCreator.DocumentDimensions = new SizeF
                        ((float)gear.AddendumCircleDiameter, (float)gear.AddendumCircleDiameter);
                    svgCreator.DocumentDimensionUnits = "mm";
                    svgCreator.ViewBoxDimensions = new RectangleF(
                        -svgCreator.DocumentDimensions.Width / 2f,
                        -svgCreator.DocumentDimensions.Width / 2f,
                        svgCreator.DocumentDimensions.Width, svgCreator.DocumentDimensions.Height);
                    svgCreator.ViewBoxDimensionUnits = "";
                    using StreamWriter sw = new StreamWriter
                        ($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}.svg");
                    sw.Write(svgCreator.ToString());
                    sw.Close();

                    // Create the output plot file of the gear

                    int limit = gear.ToothCount;
                    double angle = Math.PI;
                    if (args[0] == "-P")
                    {
                        limit = 1;
                        angle = gear.ToothAngle / 2;
                    }

                    List<IEnumerable<PointF>> gearPoints = new List<IEnumerable<PointF>>
                    {
                        Involutes.CirclePoints(-angle, angle, Math.PI / 2880, gear.PitchCircleDiameter / 2),
                        Involutes.CirclePoints(-angle, angle, Math.PI / 2880, gear.BaseCircleDiameter / 2)
                    };
                    //IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.AddendumCircleDiameter / 2);
                    //IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.DedendumCircleDiameter / 2);
                    for (int i = 0; i < limit; i++)
                    {
                        gearPoints.Add(gear.AntiClockwiseInvolute(i));
                        gearPoints.Add(gear.ClockwiseInvolute(i));
                        gearPoints.Add(gear.AnticlockwiseUndercut(i));
                        gearPoints.Add(gear.ClockwiseUndercut(i));
                        gearPoints.Add(gear.Dedendum(i));
                        gearPoints.Add(gear.Addendum(i));
                        // Remove next two lines for normal use. Used to test whether crossover detection working.
                        //gearPoints.Add(gear.ComputeInvolutePoints());
                        //gearPoints.Add(gear.ComputeUndercutPoints());
                    }

                    if (cutouts != null)
                        foreach (List<PointF> cutout in cutouts)
                            gearPoints.Add(cutout);
                    gearPoints.Add(cutoutCalculator.CalculateInlay());
                    gearPoints.Add(cutoutCalculator.CalculateSpindle());

                    using Image img = Plot.PlotGraphs(gearPoints, 2048, 2048);
                    img.Save($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}.png", ImageFormat.Png);
                }
            }
        }

        public static string GenerateGearTables(IList<int> angles, IList<int> teeth, int module, int cutterDiameter)
        {
            using StringWriter sw = new StringWriter();
            foreach (int pa in angles)
            {
                sw.WriteLine($"PRESSURE ANGLE {pa / 10.0:F1} DEGREES");
                sw.WriteLine($"MODULE {module / 100.0:F2}mm, CUTTER DIAMETER {cutterDiameter / 100.0:F2}mm");
                for (double s = 0; s <= 0.211; s += 0.03)
                {
                    sw.WriteLine($"CONTACT RATIO FOR PROFILE SHIFT {s * 200}%");
                    sw.Write("TEETH");
                    foreach (int t in teeth)
                        sw.Write($"{t,3}     ");
                    sw.WriteLine();

                    sw.Write("GAP ");
                    List<GearParameters> gears = new List<GearParameters>();
                    foreach (int i in teeth)
                    {
                        GearParameters gear = new GearParameters(i, module / 100.0, Math.PI * pa / 1800.0, s, 0, 0, cutterDiameter / 100.0);
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
                        sw.Write($"{gear.CutterAdjustedDedendumCircleDiameter,7:F3} ");
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
}
