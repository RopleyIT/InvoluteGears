using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace Plotter;

public enum StrokeType
{
    Move,
    Line,
    Cubic,
    Quadratic,
    Arc,
    Z
};

public class SVGPath : IRenderable
{
    /// <summary>
    /// The pen or line colour for the path or shape
    /// </summary>

    public string Stroke { get; set; } = String.Empty;

    /// <summary>
    /// The thickness of the line
    /// </summary>

    public string StrokeWidth { get; set; } = String.Empty;

    /// <summary>
    /// The fill colour
    /// </summary>

    public string Fill { get; set; } = String.Empty;

    public LineCap Cap { get; set; } = LineCap.None;

    public LineJoin Join { get; set; } = LineJoin.None;

    /// <summary>
    /// The lengths of the dashes and gaps for a line pattern
    /// </summary>

    public IEnumerable<int> Dashes { get; set; } = Enumerable.Empty<int>();

    /// <summary>
    /// Alternative helper function for setting the dash lengths
    /// </summary>
    /// <param name="dashes">Sequence of integers for dash lengths
    /// </param>

    public void SetDashes(params int[] dashes)
        => Dashes = dashes;

    /// <summary>
    /// Create a closed or open path using a sequence of points
    /// </summary>
    /// <param name="points">The sequence of points to be joined
    /// using straight line segments</param>
    /// <param name="closed">True to close the 
    /// last point back to the first with a line</param>
    /// <returns>The path created</returns>

    public static SVGPath FromPoints(IEnumerable<Coordinate> points, bool closed)
        => new(points, closed);

    public IList<SVGPathElement> Elements { get; private set; }
        = new List<SVGPathElement>();

    /// <summary>
    /// The current drawing position in the path
    /// </summary>

    public Coordinate CurrentPoint
    {
        get
        {
            for (int index = Elements.Count - 1; index >= 0; index--)
            {
                if (Elements[index].StrokeType != StrokeType.Z)
                    return Elements[index].Points.Last();
            }
            return Coordinate.Empty;
        }
    }

    public SVGPath() { }

    public SVGPath(IEnumerable<Coordinate> points, bool closed)
    {
        LinesThrough(points, true, closed);
    }

    public SVGPath(DrawablePath path)
    {
        if (path == null || path.Curves == null || path.Curves.Count == 0)
            throw new ArgumentException("Empty or null path");
        MoveTo(path.Curves[0].Start);
        foreach (IDrawable d in path.Curves)
            AppendDrawable(d);
        if (path.Closed)
            Close();
    }

    private void AppendDrawable(IDrawable d)
    {
        if (d is Line m)
            LineTo(m.End);
        else if (d is PolyLine p)
            LinesThrough(p.Vertices.Skip(1), false, false);
        else if (d is CubicSpline c)
            Cubic(c.Points[1].X, c.Points[1].Y,
                c.Points[2].X, c.Points[2].Y,
                c.Points[3].X, c.Points[3].Y);
        else if (d is QuadraticSpline q)
            Quadratic(q.Points[1].X, q.Points[1].Y,
                q.Points[2].X, q.Points[2].Y);
        else if (d is CircularArc a)
        {
            // Translate the arc data into
            // the bizarre SVG arc format

            //Coordinate startVec = Coordinate
            //    .FromPolar(a.Radius, a.StartAngle);
            //Coordinate startPoint = a.Centre 
            //    + startVec;
            double sweptAngle = Geometry
                .NormaliseAngle(a.EndAngle - a.StartAngle);
            Coordinate endPoint
                = a.Centre + Coordinate.FromPolar(a.Radius, a.EndAngle);
            Arc(a.Radius, a.Radius, 0,
                sweptAngle < Math.PI != a.Anticlockwise,
                a.Anticlockwise, endPoint.X, endPoint.Y);
        }
    }

    public void SetDrawingParams(string strokeColour, double strokeWidth, string fillColour)
    {
        Stroke = strokeColour;
        StrokeWidth = strokeWidth.ToString();
        Fill = fillColour;
    }

    public void LinesThrough(IEnumerable<Coordinate> points, bool moveToFirst, bool closed)
    {
        if (points == null || !points.Any())
            throw new ArgumentException("No points for SVG path");

        Coordinate prev = points.First();
        if (moveToFirst)
            Elements.Add(SVGPathElement.MoveTo(prev));
        else
            Elements.Add(SVGPathElement.LineTo(prev));

        foreach (var p in points.Skip(1))
        {
            if (p != prev)
                Elements.Add(SVGPathElement.LineTo(p));
            prev = p;
        }
        if (closed)
            Elements.Add(SVGPathElement.Close());
    }

    public void MoveTo(Coordinate p)
    => Elements.Add(SVGPathElement.MoveTo(p.X, p.Y));

    public void MoveRel(double dx, double dy)
        => Elements.Add(SVGPathElement.MoveRel(dx, dy));

    public void LineTo(Coordinate p)
        => Elements.Add(SVGPathElement.LineTo(p));

    public void LineTo(double x, double y)
        => Elements.Add(SVGPathElement.LineTo(x, y));

    public void LineRel(double dx, double dy)
        => Elements.Add(SVGPathElement.LineRel(dx, dy));

    public void Close()
        => Elements.Add(SVGPathElement.Close());

    public void Cubic(double cx1, double cy1, double cx2, double cy2, double x, double y)
        => Elements.Add(SVGPathElement.Cubic(cx1, cy1, cx2, cy2, x, y));

    public void CubicRel(double dx1, double dy1, double dx2, double dy2, double dx, double dy)
        => Elements.Add(SVGPathElement.CubicRel(dx1, dy1, dx2, dy2, dx, dy));

    public void Quadratic(double cx1, double cy1, double x, double y)
        => Elements.Add(SVGPathElement.Quadratic(cx1, cy1, x, y));

    public void QuadraticRel(double dx1, double dy1, double dx, double dy)
        => Elements.Add(SVGPathElement.QuadraticRel(dx1, dy1, dx, dy));

    public void Arc(double rx, double ry, double angle, bool largeArc, bool sweep, double x, double y)
        => Elements.Add(SVGPathElement.Arc(rx, ry, angle, largeArc, sweep, x, y));

    public void ArcRel(double rx, double ry, double angle, bool largeArc, bool sweep, double dx, double dy)
        => Elements.Add(SVGPathElement.ArcRel(rx, ry, angle, largeArc, sweep, dx, dy));

    public Rectangle BoundingBox()
    {
        Rectangle bbox = Rectangle.Empty;
        var elementsWithPoints =
            from e in Elements
            where e.Points != null && e.Points.Count > 0
            select e;

        if (elementsWithPoints.Any())
        {
            bbox = elementsWithPoints.First().BoundingBox();
            foreach (SVGPathElement e in elementsWithPoints.Skip(1))
                bbox = SVGPath.Union(bbox, e.BoundingBox());
        }
        return bbox;
    }

    public static Rectangle Union(Rectangle r, Rectangle s)
    {
        double xMax = Math.Max(r.Right, s.Right);
        double xMin = Math.Min(r.Left, s.Left);
        double yMax = Math.Max(r.Bottom, s.Bottom);
        double yMin = Math.Min(r.Top, s.Top);
        return new Rectangle
            (new(xMin, yMin), xMax - xMin, yMax - yMin);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.Write("<path d=\"");
        //int i = 0;
        foreach (SVGPathElement pe in Elements)
        {
            sw.Write(pe.ToString());
            //if (++i % 10 == 0)
            //    sw.WriteLine();
            sw.WriteLine();
        }
        sw.Write("\"");
        sw.Write(RenderStye());
        sw.WriteLine("/>");
        return sw.ToString();
    }

    public string RenderStye()
    {
        StringWriter sw = new();
        if (!string.IsNullOrEmpty(Stroke))
            sw.Write($" stroke=\"{Stroke}\"");
        if (!string.IsNullOrEmpty(Fill))
            sw.Write($" fill=\"{Fill}\"");
        if (!string.IsNullOrEmpty(StrokeWidth))
            sw.Write($" stroke-width=\"{StrokeWidth}\"");
        var strCap = Cap switch
        {
            LineCap.Round => "round",
            LineCap.Square => "square",
            LineCap.Butt => "butt",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strCap))
            sw.Write($" stroke-linecap=\"{strCap}\"");
        var strJoin = Join switch
        {
            LineJoin.Round => "round",
            LineJoin.Mitre => "mitre",
            LineJoin.Bevel => "bevel",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strJoin))
            sw.Write($" stroke-linejoin=\"{strJoin}\"");
        if (Dashes.Any())
        {
            sw.Write($" stroke-dasharray=\"{Dashes.First()}");
            foreach (int i in Dashes.Skip(1))
                sw.Write($",{i}");
            sw.Write("\"");
        }
        return sw.ToString();
    }
}
