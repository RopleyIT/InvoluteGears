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
                if(!int.TryParse(args[0], out teeth) || teeth < 5)
                {
                    Console.WriteLine("Usage: InvoluteConsole [tooth count] [% of module shift]\r\n\twhere tooth count is digits\r\n% of module shift is two digits");
                    return;
                }
            if(args.Length > 1)
                if (!int.TryParse(args[1], out shiftPercent))
                {
                    Console.WriteLine("Usage: InvoluteConsole [tooth count] [% of module shift]\r\n\twhere tooth count is digits\r\n% of module shift is two digits");
                    return;
                }
            GearParameters gear = new GearParameters(teeth, 4.0, Math.PI / 9, shiftPercent/100.0);
                    

            Plot p = new Plot();
            IEnumerable<PointF> pitchCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.PitchCircleDiameter / 2);
            IEnumerable<PointF> baseCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.BaseCircleDiameter / 2);
            IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.AddendumCircleDiameter / 2);
            IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.DedendumCircleDiameter / 2);
            var leftInvolute = gear.AntiClockwiseInvolute(0);
            var rightInvolute = gear.ClockwiseInvolute(0);
            var anticlockwiseUndercut = gear.AnticlockwiseUndercut(0);
            var clockwiseUndercut = gear.ClockwiseUndercut(0);
            Image img = p.PlotGraphs(new List<IEnumerable<PointF>> 
            {   dedendCircle, baseCircle, pitchCircle, addendCircle, 
                leftInvolute, rightInvolute, 
                clockwiseUndercut, anticlockwiseUndercut }, 2048, 2048);
            img.Save($"C:\\Course\\involute{gear.ToothCount}-{shiftPercent}.bmp", ImageFormat.Bmp);

            // Contact ratio calculations

            GearParameters gear1;
            GearParameters gear2;
            for(int s = 0; s <= 21; s += 3)
                for (int i = 10; i <= 18; i++)
                    for(int j = 36; j <= 50; j++)
                    {
                        gear1 = new GearParameters(i, 1.0, Math.PI / 9, s/100.0);
                        gear2 = new GearParameters(j, 1.0, Math.PI / 9, s/100.0);
                        double undercutContactRatio = gear1.ContactRatioWith(gear2);
                        double minGap = gear1.ToothGapAtUndercut;
                        Console.WriteLine($"{s*2}%\t{i}\t{j}\t{minGap:F3}\t{undercutContactRatio:F3}");
                    }
            for (int s = 0; s <= 21; s += 3)
            {
                gear1 = new GearParameters(720, 1.0, Math.PI / 9, s / 100.0);
                Console.WriteLine($"{s}\t{gear1.ToothGapAtUndercut:F3}");
            }
        }
    }
}
