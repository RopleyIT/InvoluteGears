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

            int teeth = 12;
            int shiftPercent = 0;
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
                    Console.WriteLine("-c gear1Teeth gear2Teeth -- contact ratios for various profile shifts");
                    Console.WriteLine("\tgear1Teeth, gear2Teeth: number of teeth on each mating gear");
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
                if(args[0] == "-c")
                {
                    if (args.Length != 3 ||
                        !int.TryParse(args[1], out int gteeth1) || !int.TryParse(args[2], out int gteeth2))
                    {
                        Console.WriteLine("Usage: InvoluteConsole -c 12 48\r\n\tWhere 12 and 48 are teeth on mating gears");
                        return;
                    }
                    GearParameters gear1;
                    GearParameters gear2;
                    for (int s = 0; s <= 21; s += 3)
                    {
                        gear1 = new GearParameters(gteeth1, 1.0, Math.PI / 9, s / 100.0);
                        gear2 = new GearParameters(gteeth2, 1.0, Math.PI / 9, s / 100.0);
                        double undercutContactRatio = gear1.ContactRatioWith(gear2);
                        double minGap = gear1.ToothGapAtUndercut;
                        Console.WriteLine($"{s * 2}%\t{gteeth1}\t{gteeth2}\t{minGap:F3}\t{undercutContactRatio:F3}");
                    }
                    return;
                }
                if (args[0] == "-p")
                {
                    if (args.Length != 7 
                        || !int.TryParse(args[1], out teeth) 
                        || !int.TryParse(args[2], out shiftPercent)
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
                        shiftPercent / 1000.0, 
                        maxErr/1000.0, 
                        backlash/1000.0);

                    Plot p = new Plot();
                    IEnumerable<PointF> pitchCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.PitchCircleDiameter / 2);
                    IEnumerable<PointF> baseCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.BaseCircleDiameter / 2);
                    IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.AddendumCircleDiameter / 2);
                    IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.DedendumCircleDiameter / 2);
                    var leftInvolute = gear.AntiClockwiseInvolute(0);
                    var rightInvolute = gear.ClockwiseInvolute(0);
                    var anticlockwiseUndercut = gear.AnticlockwiseUndercut(0);
                    var clockwiseUndercut = gear.ClockwiseUndercut(0);
                    Image img = p.PlotGraphs(new List<IEnumerable<PointF>> {   dedendCircle, baseCircle, pitchCircle, addendCircle,
                        leftInvolute, rightInvolute, clockwiseUndercut, anticlockwiseUndercut },
                        2048, 2048);
                    img.Save($"C:\\Course\\involute{gear.ToothCount}-{shiftPercent}.bmp", ImageFormat.Bmp);
                }
            }
        }
    }
}
