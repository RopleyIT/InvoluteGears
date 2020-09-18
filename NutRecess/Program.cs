using System;
using System.Drawing;
using System.Collections.Generic;
using Plotter;
using System.IO;

namespace NutRecess
{
    /// <summary>
    /// Given hex headed nuts which we would like to recess into panels, it is not possible to cut out
    /// a hexagonal inset with a circular end mill. What we do instead is cut out a triangle with
    /// rounded vertices, the rounding being at least the diameter of the end mill.
    /// 
    /// Given the distance across the flats of the nut to be inlaid, and the diameter of the
    /// desired hole coaxial with the nut's threaded hole, prepare an SVG for the hole and
    /// rounded triangular inlay that can be imported into a CAD tool for use in a design.
    /// 
    /// Usage:
    /// 
    /// nutrecess distance-across-flats-in-mm hole-diameter-in-mm destination-file-path
    /// </summary>
    
    class Program
    {
        private static readonly double Root3 = Math.Sqrt(3);

        static void Main(string[] args)
        {
            // Validate arguments

            if (args.Length != 3)
                Usage("nutrecess [flat-distance] [hole-diameter] [destination]");
            if (!double.TryParse(args[0], out double flats))
                Usage("Distance across flats must be digits with possible decimal point");
            if (!double.TryParse(args[1], out double holeDiameter))
                Usage("Hole diameter must be digits with possible decimal point");
            if (flats <= 0)
                Usage("Distance across flats must be non-zero and positive");
            if (holeDiameter < 0)
                Usage("Hole diameter must be positive, or set to zero for no hole");
            if (!args[2].EndsWith(".svg", StringComparison.CurrentCultureIgnoreCase))
                Usage("Destination file must have a '.svg' extension");

            // Create the cutout and its centering crosshairs

            var svgCreator = new SVGCreator
            {
                DocumentDimensions = NewSz(flats * 2, flats * 2),
                DocumentDimensionUnits = "mm",
                ViewBoxDimensions = NewRect(-flats, -flats, 2 * flats, 2 * flats)
            };

            svgCreator.AddClosedPath(NutRecessPoints(flats), "black", 0.03, "gray");
            if(holeDiameter > 0)
                svgCreator.AddClosedPath(Arc(0, 360, holeDiameter/2, PointF.Empty), "black", 0.03, "darkgray");
            var pathX = new SVGPath(Line(-2 * flats / 3.0, 0, 2 * flats / 3.0, 0), false);
            pathX.SetDrawingParams("black", 0.03, "");
            svgCreator.AddPath(pathX);
            var pathY = new SVGPath(Line(0, -2 * flats / 3.0, 0, 2 * flats / 3.0), false);
            pathY.SetDrawingParams("black", 0.03, "");
            svgCreator.AddPath(pathY);

            // Save out to the destination file

            using TextWriter tw = new StreamWriter(args[2]);
            tw.Write(svgCreator.ToString());
        }

        private static IEnumerable<PointF> Line(double x1, double y1, double x2, double y2)
        {
            yield return NewPt(x1, y1);
            yield return NewPt(x2, y2);
        }

        private static IEnumerable<PointF> Arc(double startAngle, double endAngle, double radius, PointF origin)
        {
            for (double a = startAngle; a <= endAngle; a += 0.1)
                yield return NewPt(radius * Math.Cos(DegToRad(a)) + origin.X, 
                    radius * Math.Sin(DegToRad(a)) + origin.Y);
        }

        private static IEnumerable<PointF> NutRecessPoints(double flats)
        {
            foreach (var p in Arc(150, 270, flats / 3, NewPt(-flats / (2 * Root3), -flats / 6)))
                yield return p;
            foreach (var p in Arc(-90, 30, flats / 3, NewPt(flats / (2 * Root3), -flats / 6)))
                yield return p;
            foreach (var p in Arc(30, 150, flats / 3, NewPt(0, flats / 3)))
                yield return p;
        }

        private static double DegToRad(double v) => v * Math.PI / 180.0;

        private static void Usage(string v)
        {
            Console.WriteLine($"Usage: {v}");
            Environment.Exit(-1);
        }

        private static PointF NewPt(double x, double y) => new PointF((float)x, (float)y);

        private static SizeF NewSz(double x, double y) => new SizeF((float)x, (float)y);

        private static RectangleF NewRect(double x, double y, double w, double h) =>
            new RectangleF(NewPt(x, y), NewSz(w, h));
    }
}
