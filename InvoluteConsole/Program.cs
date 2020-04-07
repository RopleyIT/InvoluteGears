using System;
using Plotter;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using InvoluteGears;

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
                        "-p [tooth count] [profile shift] [tolerance] [angle] [module] [backlash]\r\n"
                            + "\twhere tooth count is digits\r\n"
                            + "\tprofile shift is in thousandths of the module\r\n"
                            + "\ttolerance is in 1000ths of mm\r\n"
                            + "\tangle is pressure angle in 10ths of a degree\r\n"
                            + "\tmodule is in 100ths of a mm\r\n"
                            + "\tbacklash is in thousandths of the module");
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
                if (args[0] == "-p")
                {
                    if (args.Length != 7
                        || !int.TryParse(args[1], out int teeth)
                        || !int.TryParse(args[2], out int shift)
                        || !int.TryParse(args[3], out int maxErr)
                        || !int.TryParse(args[4], out int pressureAngle)
                        || !int.TryParse(args[5], out int module)
                        || !int.TryParse(args[6], out int backlash))
                    {
                        Console.WriteLine("Usage: InvoluteConsole -p [tooth count] [profile shift] [tolerance] [angle] [module] [backlash]\r\n"
                            + "\twhere tooth count is digits\r\n"
                            + "\tprofile shift is in thousandths of the module\r\n"
                            + "\ttolerance is in 1000ths of mm\r\n"
                            + "\tangle is pressure angle in 10ths of a degree\r\n"
                            + "\tmodule is in 100ths of a mm\r\n"
                            + "\tbacklash is in thousandths of the module");
                        return;
                    }
                    GearParameters gear = new GearParameters(
                        teeth, 
                        module/100.0, 
                        Math.PI * pressureAngle/1800.0, 
                        shift / 1000.0, 
                        maxErr/1000.0, 
                        backlash/1000.0);

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
                    Image img = p.PlotGraphs(gearPoints, 2048, 2048);
                    img.Save($"t{gear.ToothCount}p{shift}a{pressureAngle}m{module}b{backlash}.bmp", ImageFormat.Bmp);
                }
            }
        }
    }
}
