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
            if (args.Length > 0)
                if(!int.TryParse(args[0], out teeth) || teeth < 5)
                {
                    Console.WriteLine("Usage: InvoluteConsole [tooth count]\r\n\twhere tooth count is digits");
                    return;
                }
            GearParameters gear = new GearParameters(teeth, 4.0, Math.PI / 9);
                    

            Plot p = new Plot();
            IEnumerable<PointF> pitchCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.PitchCircleDiameter / 2);
            IEnumerable<PointF> baseCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.BaseCircleDiameter / 2);
            IEnumerable<PointF> addendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.PitchCircleDiameter / 2 + gear.Module);
            IEnumerable<PointF> dedendCircle = Involutes.CirclePoints(-Math.PI / gear.ToothCount, Math.PI / gear.ToothCount, Math.PI / 2880, gear.PitchCircleDiameter / 2 - gear.Module);
            var leftInvolute = gear.AntiClockwiseInvolute(0);
            var rightInvolute = gear.ClockwiseInvolute(0);
            var anticlockwiseUndercut = gear.AnticlockwiseUndercut(0);
            var clockwiseUndercut = gear.ClockwiseUndercut(0);
            Image img = p.PlotGraphs(new List<IEnumerable<PointF>> 
            {   dedendCircle, baseCircle, pitchCircle, addendCircle, 
                leftInvolute, rightInvolute, 
                clockwiseUndercut, anticlockwiseUndercut }, 2048, 2048);
            img.Save($"C:\\Course\\involute{gear.ToothCount}.bmp", ImageFormat.Bmp);

            // Show the gap between teeth as this determines the cutter cross section

            Console.WriteLine($"Minimum tooth gap {gear.ToothGapAtUndercut}");

            // Contact ratio calculations

            GearParameters gear1;
            GearParameters gear2;
            for (int i = 6; i < 12; i++)
                for(int j = i; j < 12; j++)
                {
                    gear1 = new GearParameters(i, 1.0, Math.PI / 9);
                    gear2 = new GearParameters(j, 1.0, Math.PI / 9);
                    double contactRatio = Involutes.IdealContactRatio(gear1, gear2);
                    Console.WriteLine($"{i}\t{j}\t{contactRatio}");
                }
        }
    }
}
