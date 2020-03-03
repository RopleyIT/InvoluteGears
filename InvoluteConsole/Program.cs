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
            GearParameters gear = new GearParameters
            {
                Module = 4, // 4 millimetres
                PressureAngle = Math.PI / 9, // 20 degrees
                ToothCount = 12
            };

            int teeth = 0;
            if (args.Length > 0)
                if(int.TryParse(args[0], out teeth) && teeth >= 4)
                    gear.ToothCount = teeth;

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

            // Contact ratio calculations

            GearParameters gear1 = new GearParameters
            {
                Module = 1,
                PressureAngle = Math.PI / 9,
                ToothCount = 8
            };

            GearParameters gear2 = new GearParameters
            {
                Module = 1,
                PressureAngle = Math.PI / 9,
                ToothCount = 8
            };

            for (int i = 6; i < 12; i++)
                for(int j = i; j < 12; j++)
                {
                    gear1.ToothCount = i;
                    gear2.ToothCount = j;
                    double contactRatio = Involutes.ContactRatio(gear1, gear2);
                    Console.WriteLine($"{i}\t{j}\t{contactRatio}");
                }
        }
    }
}
