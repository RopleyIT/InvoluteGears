using System;
using Plotter;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using InvoluteGears;
using System.IO;

namespace InvoluteConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if(args[0] == "-h")
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
                if(args[0] == "-C")
                {
                    if(args.Length != 2)
                    {
                        Console.WriteLine("Usage: gears -C output-file-path -- contact ratios for various profile shifts and angles");
                        Console.WriteLine("\toutput-file-path: Where to store the results");
                        return;
                    }
                    List<GearParameters> gears;
                    using(StreamWriter sw = new StreamWriter(args[1]))
                    {
                        foreach (int pa in new int[] { 145, 200, 250 })
                        {
                            sw.WriteLine($"PRESSURE ANGLE {pa / 10.0:F1} DEGREES");
                            for (double s = 0; s <= 0.211; s += 0.03)
                            {
                                var teeth = new int[] { 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 };
                                sw.WriteLine($"CONTACT RATIO FOR PROFILE SHIFT {s * 200}%");
                                sw.WriteLine("TEETH  10      12      14      16      18      24      30      36      48      72     144");

                                sw.Write("GAP ");
                                gears = new List<GearParameters>();
                                foreach (int i in teeth)
                                {
                                    var gear = new GearParameters(i, 1.0, Math.PI * pa / 1800.0, s);
                                    sw.Write($"{gear.ToothGapAtUndercut,7:F3} ");
                                    gears.Add(gear);
                                }
                                sw.WriteLine();

                                sw.Write("Db  ");
                                foreach (var gear in gears)
                                    sw.Write($"{gear.BaseCircleDiameter,7:F3} ");
                                sw.WriteLine();

                                sw.Write("Dd  ");
                                foreach (var gear in gears)
                                    sw.Write($"{gear.DedendumCircleDiameter,7:F3} ");
                                sw.WriteLine();

                                sw.Write("Du  ");
                                foreach (var gear in gears)
                                    sw.Write($"{gear.UndercutRadius * 2,7:F3} ");
                                sw.WriteLine();

                                sw.Write("Dp  ");
                                foreach (var gear in gears)
                                    sw.Write($"{gear.PitchCircleDiameter,7:F3} ");
                                sw.WriteLine();

                                sw.Write("Da  ");
                                foreach (var gear in gears)
                                    sw.Write($"{gear.AddendumCircleDiameter,7:F3} ");
                                sw.WriteLine();

                                for (int i = 0; i < teeth.Length; i++)
                                {
                                    sw.Write($"{teeth[i],3} ");
                                    for (int j = 0; j < teeth.Length; j++)
                                        if (j < i)
                                            sw.Write(".       ");
                                        else
                                            sw.Write($"{gears[i].ContactRatioWith(gears[j]),7:F3} ");
                                    sw.WriteLine();
                                }
                                sw.WriteLine();
                            }
                        }
                    }
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
                        module/100.0, 
                        Math.PI * pressureAngle/1800.0, 
                        shift / 1000.0, 
                        maxErr/100.0, 
                        backlash/1000.0,
                        cutterDiameter/100.0);

                    // Generate the SVG version of the gear path

                    SVGPath svgPath = new SVGPath(gear.GenerateCompleteGearPath(), true);
                    SVGCreator svgCreator = new SVGCreator(svgPath);
                    //svgCreator.DocumentDimensions = new SizeF
                    //    ((float)(gear.Module * (gear.ToothCount + 2)), (float)(gear.Module * (gear.ToothCount + 2)));
                    //svgCreator.DocumentDimensionUnits = "mm";
                    svgCreator.DocumentDimensions = new SizeF
                        ((float)gear.AddendumCircleDiameter, (float)gear.AddendumCircleDiameter);
                    svgCreator.DocumentDimensionUnits = "mm";
                    svgCreator.ViewBoxDimensions = new RectangleF(
                        -(float)svgCreator.DocumentDimensions.Width / 2f,
                        -(float)svgCreator.DocumentDimensions.Width / 2f,
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

                    Plot p = new Plot();
                    var gearPoints = new List<IEnumerable<PointF>>();
                    gearPoints.Add(Involutes.CirclePoints(-angle, angle, Math.PI / 2880, gear.PitchCircleDiameter / 2));
                    gearPoints.Add(Involutes.CirclePoints(-angle, angle, Math.PI / 2880, gear.BaseCircleDiameter / 2));
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
                    using Image img = p.PlotGraphs(gearPoints, 2048, 2048);
                    img.Save($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}.png", ImageFormat.Png);
                }
            }
        }
    }
}
