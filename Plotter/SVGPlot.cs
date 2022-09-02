using System;
using System.Collections.Generic;
//using System.Drawing.Drawing2D;
using System.Linq;
using TwoDimensionLib;

namespace Plotter
{
    public class SVGPlot
    {
        /// <summary>
        /// Calculate the number of pixels to use for each unit on the graph. Used to
        /// manage the scaling of line thicknesses and fonts.
        /// </summary>
        /// <param name="bounds">The max and min values for X and Y in the plot</param>
        /// <param name="width">The width in logical pixels for the viewbox to use</param>
        /// <param name="height">The hieght in logical pixels for the viewbox to use</param>
        /// <returns>The scale factor to use</returns>
        
        private static double ScaleFactor(Rectangle bounds, int width, int height)
        {
            double scaleY = height / bounds.Height;
            double scaleX = width / bounds.Width;
            return Math.Min(scaleX, scaleY);
        }

        private static string[] colours =
        {
                "gold",
                "darkgreen",
                "darkred",
                "cornflowerblue",
                "darkorange",
                "green",
                "red",
                "cadetblue",
                "yellow",
                "greenyellow",
                "tomato",
                "blue",
                "coral",
                "olive",
                "salmon",
                "darkblue",
                "darkmagenta"
        };

        public static string PlotGraphs(IEnumerable<IEnumerable<Coordinate>> points, int width, int height, string color = null)
        {
            string[] usedColours = colours;
            if (!string.IsNullOrWhiteSpace(color))
                usedColours = new string[] { color };

            SVGCreator svg = new SVGCreator();

            // Turn off header, width and height

            svg.HasXmlHeader = false;
            svg.HasWidthAndHeight = false;

            BoundsTracker bounds = new();
            List<List<Coordinate>> plots = new();
            foreach (IEnumerable<Coordinate> pl in points)
                plots.Add(bounds.Track(pl).ToList());
            double scale = ScaleFactor(bounds.Bounds, width, height);
            PlotAxes(bounds, scale, svg);
            int index = 0;
            foreach (List<Coordinate> pl in plots)
                PlotGraph(pl, svg, bounds.Bounds, scale, 
                    usedColours[index++ % usedColours.Length]);
            //svg.DocumentDimensions = new Coordinate(600, 600);
            //svg.DocumentDimensionUnits = "px";
            svg.ViewBoxDimensions = bounds.Bounds;
            //svg.ViewBoxDimensionUnits = "mm";

            return svg.ToString();
        }

        private static void PlotAxes(BoundsTracker bounds, double scale, SVGCreator svg)
        {
            double unitsX = UnitSize(bounds.Bounds.Width);
            double unitsY = UnitSize(bounds.Bounds.Height);

            for (double v = RoundUp(bounds.Bounds.Left, unitsX); v < bounds.Bounds.Right; v += unitsX)
            {
                List<Coordinate> rule = new()
                {
                    new Coordinate(v, bounds.Bounds.Top),
                    new Coordinate(v, bounds.Bounds.Bottom)
                };

                PlotGraph(rule, svg, bounds.Bounds, scale, "lightgray");
                LabelXRule(v, svg, unitsY);
            }
            for (double v = RoundUp(bounds.Bounds.Top, unitsY); v < bounds.Bounds.Bottom; v += unitsY)
            {
                List<Coordinate> rule = new()
                {
                    new Coordinate(bounds.Bounds.Left, v),
                    new Coordinate(bounds.Bounds.Right, v)
                };

                PlotGraph(rule, svg, bounds.Bounds, scale, "lightgray");
                if(v != 0)
                    LabelYRule(v, svg, unitsX);
            }
        }

        private static void LabelXRule(double v, SVGCreator svg, double units)
        {
            // First generate label string

            string label = v.ToString("G4");
            var txt = svg.AddText(label, new(v, 0), $"{units / 6}px") as SvgText;
            txt.Alignment = SvgText.Centre | SvgText.Top;
        }

        private static void LabelYRule(double v, SVGCreator svg, double units)
        {
            // First generate label string

            string label = v.ToString("G4");
            var txt = svg.AddText(label, new(0, v), $" {units / 6}px") as SvgText;
            txt.Alignment = SvgText.LCaseMiddle;
        }

        private static double RoundUp(double x, double unitsX) => Math.Ceiling(x / unitsX) * unitsX;
        private static double UnitSize(double range)
        {
            double log = Math.Log10(range);
            double exponent = Math.Floor(log);
            double baseUnit = Math.Pow(10.0, exponent - 1);
            double mantissa = log - exponent;
            if (mantissa <= Math.Log10(2))
                return 2 * baseUnit;
            if (mantissa <= Math.Log10(5))
                return 5 * baseUnit;
            return 10 * baseUnit;
        }

        public static string PlotGraph(List<Coordinate> points, int width, int height)
        {
            List<List<Coordinate>> pointLists = new() { points };
            return PlotGraphs(pointLists, width, height);
        }

        private static void PlotGraph(List<Coordinate> points, SVGCreator svg,
            Rectangle bounds, double scale, string penColor)
        {
            var ir = svg.AddPath(points, false, penColor, 1.0/scale, "transparent");
            ir.Join = LineJoin.Round;
        }
    }
}
