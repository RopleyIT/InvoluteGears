using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Plotter
{
    public enum StrokeType
    {
        Move,
        Line,
        Cubic,
        Quadratic,
        Arc,
        Z
    };

    public class SVGPath
    {
        public string Stroke { get; private set; }
        public string StrokeWidth { get; private set; }
        public string Fill { get; private set; }

        public IList<SVGPathElement> Elements { get; private set; }
            = new List<SVGPathElement>();

        public SVGPath(IEnumerable<PointF> points, bool closed)
        {
            if (points == null || !points.Any())
                throw new ArgumentException("No points for SVG path");

            PointF prev = points.First();
            Elements.Add(SVGPathElement.MoveTo(prev));
            foreach (PointF p in points.Skip(1))
            {
                if (p != prev)
                    Elements.Add(SVGPathElement.LineTo(p));
                prev = p;
            }
            if (closed)
                Elements.Add(SVGPathElement.Close());
        }

        public void SetDrawingParams(string strokeColour, int strokeWidth, string fillColour)
        {
            Stroke = strokeColour;
            StrokeWidth = strokeWidth.ToString();
            Fill = fillColour;
        }

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            sw.Write("<path d=\"");
            int i = 0;
            foreach (SVGPathElement pe in Elements)
            {
                sw.Write(pe.ToString());
                if (++i % 10 == 0)
                    sw.WriteLine();
            }
            sw.Write("\"");
            if (!string.IsNullOrEmpty(Stroke))
                sw.Write($" stroke=\"{Stroke}\"");
            if (!string.IsNullOrEmpty(StrokeWidth))
                sw.Write($" stroke-width=\"{StrokeWidth}\"");
            if (!string.IsNullOrEmpty(Fill))
                sw.Write($" fill=\"{Fill}\"");
            sw.WriteLine("/>");
            return sw.ToString();
        }
    }

    public class SVGPathElement
    {
        public StrokeType StrokeType { get; private set; }
        public bool Relative { get; private set; }

        private readonly PointF[] points;

        public IList<PointF> Points => points;

        private SVGPathElement(StrokeType type, bool rel)
        {
            StrokeType = type;
            Relative = rel;
            if (type == StrokeType.Cubic)
                points = new PointF[3];
            else if (type == StrokeType.Quadratic)
                points = new PointF[2];
            else if (type == StrokeType.Arc)
                points = new PointF[3];
            else if (type == StrokeType.Z)
                points = Array.Empty<PointF>();
            else
                points = new PointF[1];
        }

        public static SVGPathElement MoveTo(PointF p)
            => MoveTo(p.X, p.Y);

        public static SVGPathElement MoveTo(float x, float y)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Move, false);
            element.points[0] = new PointF(x, y);
            return element;
        }

        public static SVGPathElement MoveRel(float dx, float dy)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Move, true);
            element.points[0] = new PointF(dx, dy);
            return element;
        }

        public static SVGPathElement LineTo(PointF p)
            => LineTo(p.X, p.Y);

        public static SVGPathElement LineTo(float x, float y)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Line, false);
            element.points[0] = new PointF(x, y);
            return element;
        }

        public static SVGPathElement LineRel(float dx, float dy)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Line, true);
            element.points[0] = new PointF(dx, dy);
            return element;
        }

        public static SVGPathElement Close() => new SVGPathElement(StrokeType.Z, false);

        public static SVGPathElement Cubic(float cx1, float cy1, float cx2, float cy2, float x, float y)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Cubic, false);
            element.points[0] = new PointF(cx1, cy1);
            element.points[1] = new PointF(cx2, cy2);
            element.points[2] = new PointF(x, y);
            return element;
        }

        public static SVGPathElement CubicRel(float dx1, float dy1, float dx2, float dy2, float dx, float dy)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Cubic, true);
            element.points[0] = new PointF(dx1, dy1);
            element.points[1] = new PointF(dx2, dy2);
            element.points[2] = new PointF(dx, dy);
            return element;
        }

        public static SVGPathElement Quadratic(float cx1, float cy1, float x, float y)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Cubic, false);
            element.points[0] = new PointF(cx1, cy1);
            element.points[1] = new PointF(x, y);
            return element;
        }

        public static SVGPathElement QuadraticRel(float dx1, float dy1, float dx, float dy)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Cubic, true);
            element.points[0] = new PointF(dx1, dy1);
            element.points[1] = new PointF(dx, dy);
            return element;
        }

        public static SVGPathElement Arc(float rx, float ry, float angle, bool largeArc, bool sweep, float x, float y)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Arc, false);
            element.points[0] = new PointF(rx, ry);
            element.points[1] = new PointF(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
            element.points[2] = new PointF(x, y);
            return element;
        }

        public static SVGPathElement ArcRel(float rx, float ry, float angle, bool largeArc, bool sweep, float dx, float dy)
        {
            SVGPathElement element = new SVGPathElement(StrokeType.Arc, true);
            element.points[0] = new PointF(rx, ry);
            element.points[1] = new PointF(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
            element.points[2] = new PointF(dx, dy);
            return element;
        }

        private const string TypeStrings = "MLCQAZ";
        private const string RelTypeStrings = "mlcqaz";
        private static readonly string[] ArcStrings = { "0,0", "1,0", "0,1", "1,1" };

        public override string ToString()
        {
            string typeString = Relative ? RelTypeStrings : TypeStrings;
            string result = typeString[(int)StrokeType].ToString();
            switch (StrokeType)
            {
                case StrokeType.Move:
                case StrokeType.Line:
                    result += RenderPoint(points[0]);
                    break;
                case StrokeType.Quadratic:
                    result += RenderPoints(points, 2);
                    break;
                case StrokeType.Cubic:
                    result += RenderPoints(points, 3);
                    break;
                case StrokeType.Arc:
                    result += RenderPoint(points[0]);
                    result += $",{points[1].X:F2},{ArcStrings[(int)points[1].Y]},";
                    result += RenderPoint(points[2]);
                    break;
            }
            return result;
        }

        private static string RenderPoint(PointF p) => $"{p.X:F2},{p.Y:F2}";

        private static string RenderPoints(IEnumerable<PointF> pe, int count)
            => string.Join(",", pe.Take(count).Select(p => RenderPoint(p)));
    }
}
