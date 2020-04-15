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
                        "-p [tooth count] [profile shift] [tolerance] [angle] [module] [backlash] [cutter diameter]\r\n"
                            + "\twhere tooth count is digits\r\n"
                            + "\tprofile shift is in thousandths of the module\r\n"
                            + "\ttolerance is in 100ths of mm\r\n"
                            + "\tangle is pressure angle in 10ths of a degree\r\n"
                            + "\tmodule is in 100ths of a mm\r\n"
                            + "\tbacklash is in thousandths of the module\r\n"
                            + "\tcutter diameter is in 100ths of a mm");
                    Console.WriteLine("-m num denom teethMin, teethMax -- find gear-pinion pairs with same separation");
                    Console.WriteLine("\tnum: numerator of the overall gear ratio");
                    Console.WriteLine("\tdenom: denominator of the overall gear ratio");
                    Console.WriteLine("\tteethMin, teethMax: ranges of tooth counts for search");
                    Console.WriteLine("-c gear1Teeth gear2Teeth [pressure angle] -- contact ratios for various profile shifts");
                    Console.WriteLine("\tgear1Teeth, gear2Teeth: number of teeth on each mating gear"
                            + "\tPressure angle is measured in 10ths of a degree");
                    return;
                }
                if (args[0] == "-m")
                {
                    if (args.Length != 5 ||
                        !int.TryParse(args[1], out int numerator) || !int.TryParse(args[2], out int denominator)
                        || !int.TryParse(args[3], out int minTeeth) | !int.TryParse(args[4], out int maxTeeth))
                    {
                        Console.WriteLine("Usage: InvoluteConsole -m 13 5 10 108\r\n\tWhere 13/5 is the overall gear ratio, 10 and 108 the min and max number of teeth");
                        return;
                    }
                    Console.WriteLine(GearParameters.MatchedPairs(numerator, denominator, minTeeth, maxTeeth));
                    return;
                }
                if (args[0] == "-c")
                {
                    if (args.Length != 4 ||
                        !int.TryParse(args[1], out int gteeth1) 
                        || !int.TryParse(args[2], out int gteeth2) 
                        || !int.TryParse(args[3], out int pressureAngle))
                    {
                        Console.WriteLine("Usage: InvoluteConsole -c 12 48 [pressure angle]\r\n"
                            + "\tWhere 12 and 48 are teeth on mating gears\r\n"
                            + "\tPressure angle is measured in 10ths of a degree");
                        return;
                    }
                    GearParameters gear1;
                    GearParameters gear2;
                    Console.WriteLine("SHIFT   GEAR 1  GEAR 2  MIN GAP CR");
                    for (double s = 0; s <= 0.211; s += 0.03)
                    {
                        gear1 = new GearParameters(gteeth1, 1.0, Math.PI * pressureAngle / 1800.0, s);
                        gear2 = new GearParameters(gteeth2, 1.0, Math.PI * pressureAngle / 1800.0, s);
                        double undercutContactRatio = gear1.ContactRatioWith(gear2);
                        double minGap = gear1.ToothGapAtUndercut;
                        Console.WriteLine($"{s * 200}%\t{gteeth1}\t{gteeth2}\t{minGap:F3}\t{undercutContactRatio:F3}");
                    }
                    return;
                }
                if(args[0] == "-C")
                {
                    if(args.Length != 2)
                    {
                        Console.WriteLine("Usage: InvoluteConsole -C [output file path]\r\n"
                            + "\tWhere output file path is the path at which\r\n"
                            + "\tthe gear tables will be written");
                        return;
                    }
                    List<GearParameters> gears;
                    using(StreamWriter sw = new StreamWriter(args[1]))
                    {
                        for (double s = 0; s <= 0.211; s += 0.03)
                        {
                            var teeth = new int[] { 10, 12, 14, 16, 18, 24, 30, 36, 48, 72 };
                            sw.WriteLine($"CONTACT RATIO FOR PROFILE SHIFT {s * 200}%");
                            sw.WriteLine("TEETH  10      12      14      16      18      24      30      36      48      72");
                            
                            sw.Write("GAP ");
                            gears = new List<GearParameters>();
                            foreach (int i in teeth)
                            {
                                var gear = new GearParameters(i, 1.0, Math.PI * 200 / 1800.0, s);
                                sw.Write($"{gear.ToothGapAtUndercut:F3}\t");
                                gears.Add(gear);
                            }
                            sw.WriteLine();

                            for(int i = 0; i < teeth.Length; i++)
                            {
                                sw.Write($"{teeth[i]}  ");
                                for(int j = 0; j < teeth.Length; j++)
                                    sw.Write($"{gears[i].ContactRatioWith(gears[j]):F3}\t");
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                        }
                    }
                    return;
                }
                if (args[0] == "-p")
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
                        Console.WriteLine("Usage: InvoluteConsole -p [tooth count] [profile shift] [tolerance] [angle] [module] [backlash] [cutter diameter]\r\n"
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

                    Plot p = new Plot();
                    var gearPoints = new List<IEnumerable<PointF>>();
                    gearPoints.Add(Involutes.CirclePoints(-Math.PI, Math.PI, Math.PI / 2880, gear.PitchCircleDiameter / 2));
                    gearPoints.Add(Involutes.CirclePoints(-Math.PI, Math.PI, Math.PI / 2880, gear.BaseCircleDiameter / 2));
                    //IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.AddendumCircleDiameter / 2);
                    //IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.DedendumCircleDiameter / 2);
                    for (int i = 0; i < gear.ToothCount; i++)
                    {
                        gearPoints.Add(gear.AntiClockwiseInvolute(i));
                        gearPoints.Add(gear.ClockwiseInvolute(i));
                        gearPoints.Add(gear.AnticlockwiseUndercut(i));
                        gearPoints.Add(gear.ClockwiseUndercut(i));
                        gearPoints.Add(gear.Dedendum(i));
                        gearPoints.Add(gear.Addendum(i));
                    }
                    using Image img = p.PlotGraphs(gearPoints, 2048, 2048);
                    img.Save($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}c{cutterDiameter}.png", ImageFormat.Png);
                }
            }
        }
    }
}
