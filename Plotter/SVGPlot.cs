using System;
using System.Collections.Generic;
//using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using TwoDimensionLib;

namespace Plotter
{
    public static class SVGPlot
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

        public static string PlotCurves(DrawableSet paths,
            int width, int height, IList<string> strokes = null, 
            IList<string> fills = null)
            => PlotCurves(paths, width, height,paths.Bounds, strokes, fills);

        public static string PlotCurves(DrawableSet paths, 
            int width, int height, Rectangle bounds,
            IList<string> strokes = null, IList<string> fills = null)
        {
            if (strokes == null || fills == null
                || strokes.Count == 0
                || fills.Count == 0)
            {
                strokes = new List<string> { "black" };
                fills = new List<string> { "transparent" };
            }

            SVGCreator svg = new SVGCreator();

            // Turn off header, width and height

            svg.HasXmlHeader = false;
            svg.HasWidthAndHeight = false;

            double scale = ScaleFactor(bounds, width, height);
            PlotAxes(bounds, scale, svg);
            int index = 0;
            foreach(DrawablePath p in paths.Paths)
            {
                string stroke = strokes[index % fills.Count];
                string fill = fills[index++ % fills.Count];
                PlotCurve(p, svg, bounds, scale, stroke, fill);
            }
            svg.ViewBoxDimensions = bounds;
            return svg.ToString();
        }

        private static void PlotCurve(DrawablePath p, SVGCreator svg, 
            Rectangle bounds, double scale, string stroke, string fill)
        {
            var ir = svg.AddPath(new SVGPath(p), stroke, 1.0/scale, fill);
            ir.Join = LineJoin.Round;
        }

        public static string PlotGraphs(IEnumerable<IEnumerable<Coordinate>> points, 
            int width, int height, IList<string> strokes = null, 
            IList<string> fills = null)
        {
            if(strokes == null || fills == null
                || strokes.Count == 0
                || fills.Count == 0)
            {
                strokes = new List<string> { "black" };
                fills = new List<string> { "transparent" };
            }

            SVGCreator svg = new SVGCreator();

            // Turn off header, width and height

            svg.HasXmlHeader = false;
            svg.HasWidthAndHeight = false;

            BoundsTracker bounds = new();
            List<List<Coordinate>> plots = new();
            foreach (IEnumerable<Coordinate> pl in points)
                plots.Add(bounds.Track(pl).ToList());
            double scale = ScaleFactor(bounds.Bounds, width, height);
            PlotAxes(bounds.Bounds, scale, svg);
            int index = 0;
            foreach (List<Coordinate> pl in plots)
            {
                string stroke = strokes[index % fills.Count];
                string fill = fills[index++ % fills.Count];
                PlotGraph(pl, svg, bounds.Bounds, scale,
                    stroke, fill);
            }
            //svg.DocumentDimensions = new Coordinate(600, 600);
            //svg.DocumentDimensionUnits = "px";
            svg.ViewBoxDimensions = bounds.Bounds;
            //svg.ViewBoxDimensionUnits = "mm";

            return svg.ToString();
        }

        private static void PlotAxes(Rectangle bounds, double scale, SVGCreator svg)
        {
            double unitsX = UnitSize(bounds.Width);
            double unitsY = UnitSize(bounds.Height);

            for (double v = RoundUp(bounds.Left, unitsX); v < bounds.Right; v += unitsX)
            {
                List<Coordinate> rule = new()
                {
                    new Coordinate(v, bounds.Top),
                    new Coordinate(v, bounds.Bottom)
                };

                PlotGraph(rule, svg, bounds, scale, "lightgray", "transparent");
                LabelXRule(v, svg, unitsY);
            }
            for (double v = RoundUp(bounds.Top, unitsY); v < bounds.Bottom; v += unitsY)
            {
                List<Coordinate> rule = new()
                {
                    new Coordinate(bounds.Left, v),
                    new Coordinate(bounds.Right, v)
                };

                PlotGraph(rule, svg, bounds, scale, "lightgray", "transparent");
                if (v != 0)
                    LabelYRule(v, svg, unitsX);
            }
        }

        private static void LabelXRule(double v, SVGCreator svg, double units)
        {
            // First generate label string

            string label = v.ToString("G4");
            var txt = svg.AddText(label, new(v, 0), "gray", $"{units / 6}px") as SvgText;
            txt.Alignment = SvgText.Centre | SvgText.Top;
        }

        private static void LabelYRule(double v, SVGCreator svg, double units)
        {
            // First generate label string

            string label = v.ToString("G4");
            var txt = svg.AddText(label, new(0, v), "gray", $" {units / 6}px") as SvgText;
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

        private static void PlotGraph
            (List<Coordinate> points, SVGCreator svg,
            Rectangle bounds, double scale, string penColor, 
            string brushColor)
        {
            var ir = svg.AddPath(points, false, penColor, 1.0 / scale, brushColor);
            ir.Join = LineJoin.Round;
        }
    }
}
