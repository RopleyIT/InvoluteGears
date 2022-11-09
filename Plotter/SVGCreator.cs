using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace Plotter;

public class SVGCreator
{
    private readonly List<IRenderable> svgElements;

    public bool HasXmlHeader { get; set; } = true;

    public bool HasWidthAndHeight { get; set; } = true;

    public SVGCreator() => svgElements = new List<IRenderable>();

    private IRenderable AddElement(IRenderable element)
    {
        svgElements.Add(element);
        return element;
    }

    private IRenderable AddWithStyle
        (IRenderable r, string stroke, double strokeWidth, string fill)
    {
        r.Stroke = stroke;
        r.StrokeWidth = strokeWidth.ToString();
        r.Fill = fill;
        return AddElement(r);
    }

    public IRenderable AddPath(SVGPath path, string stroke, double strokeWidth, string fill)
       =>  AddWithStyle(path, stroke, strokeWidth, fill);
    
        public IRenderable AddPath(
        IEnumerable<Coordinate> points,
        bool close = false,
        string stroke = "black",
        double strokeWidth = 1,
        string fill = "transparent")
        => AddWithStyle(new SVGPath(points, close), stroke, strokeWidth, fill);

    public IRenderable AddEllipse(Coordinate centre, Coordinate radii, string stroke, double strokeWidth, string fill)
    {
        SVGPath path = new();
        path.MoveTo(new Coordinate(centre.X - radii.X, centre.Y));
        path.Arc(radii.X, radii.Y, 0, false, false, centre.X + radii.X, centre.Y);
        path.Arc(radii.X, radii.Y, 0, false, false, centre.X - radii.X, centre.Y);
        path.Close();
        return AddWithStyle(path, stroke, strokeWidth, fill);
    }

    public IRenderable AddText(string text, Coordinate location, string fill = "black",
        string fontSize = "20px", string fontName = "sans-serif", bool italic = false, 
        bool bold = false)
    {
        IRenderable r = new SvgText(text, location, fontSize, fontName, italic, bold);
        if (!string.IsNullOrWhiteSpace(fill))
            r.Fill = fill;
        return AddElement(r);
    }

    public IRenderable AddCircle(Coordinate centre, float radius, string stroke, double strokeWidth, string fill)
        => AddEllipse(centre, new Coordinate(radius, radius), stroke, strokeWidth, fill);

    public IRenderable AddRoundedRect(Rectangle r, Coordinate rnd, string stroke, double strokeWidth, string fill)
    {
        SVGPath path = new();
        path.MoveTo(new Coordinate(r.Left, r.Top + rnd.Y));
        path.Arc(rnd.X, rnd.Y, 0, false, true, r.Left + rnd.X, r.Top);
        path.LineTo(r.Right - rnd.X, r.Top);
        path.Arc(rnd.X, rnd.Y, 0, false, true, r.Right, r.Top + rnd.Y);
        path.LineTo(new Coordinate(r.Right, r.Bottom - rnd.Y));
        path.Arc(rnd.X, rnd.Y, 0, false, true, r.Right - rnd.X, r.Bottom);
        path.LineTo(new Coordinate(r.Left + rnd.X, r.Bottom));
        path.Arc(rnd.X, rnd.Y, 0, false, true, r.Left, r.Bottom - rnd.Y);
        path.Close();
        return AddWithStyle(path, stroke, strokeWidth, fill);
    }

    public IRenderable AddRect(Rectangle r, string stroke, double strokeWidth, string fill)
    {
        var corners = new Coordinate[]
        {
                new(r.Left, r.Top),
                new(r.Right, r.Top),
                new(r.Right, r.Bottom),
                new(r.Left, r.Bottom)
        };
        return AddPath(corners, true, stroke, strokeWidth, fill);
    }

    public IRenderable AddLine(Coordinate start, Coordinate end, string stroke, double strokeWidth)
        => AddPath(new Coordinate[] { start, end }, false, stroke, strokeWidth, null);

    public IRenderable AddPolyline(IEnumerable<Coordinate> points, string stroke, double strokeWidth)
        => AddPath(points, false, stroke, strokeWidth, null);

    public IRenderable AddPolygon(IEnumerable<Coordinate> points, string stroke, double strokeWidth, string fill)
        => AddPath(points, true, stroke, strokeWidth, fill);

    public Coordinate DocumentDimensions { get; set; } = Coordinate.Empty;

    public string DocumentDimensionUnits { get; set; } = string.Empty;

    public Rectangle ViewBoxDimensions { get; set; } = Rectangle.Empty;

    public string ViewBoxDimensionUnits { get; set; } = string.Empty;

    public string InfoComment { get; set; } = string.Empty;

    private static string XmlHeader => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>";

    private string StartSvg
    {
        get
        {
            string startSvg = string.Empty;
            if (HasXmlHeader)
                startSvg += XmlHeader;
            startSvg += "<svg ";
            if (HasWidthAndHeight)
                startSvg += $"width=\"{DocumentDimensions.X}{DocumentDimensionUnits}\" "
                        + $"height=\"{DocumentDimensions.Y}{DocumentDimensionUnits}\" ";
            startSvg += $"viewBox=\"{ViewBoxDimensions.Left}{ViewBoxDimensionUnits} "
                        + $"{ViewBoxDimensions.Top}{ViewBoxDimensionUnits} "
                        + $"{ViewBoxDimensions.Width}{ViewBoxDimensionUnits} "
                        + $"{ViewBoxDimensions.Height}{ViewBoxDimensionUnits}\"";
            if (HasXmlHeader)
                startSvg += $" xmlns=\"http://www.w3.org/2000/svg\">";
            else
                startSvg += ">";
            return startSvg;
        }
    }

    private static string EndSvg => "</svg>";

    public void CalculateViewBox(Coordinate margin)
    {
        if (svgElements != null && svgElements.Count > 0)
        {
            ViewBoxDimensions = svgElements[0].BoundingBox();
            foreach (IRenderable p in svgElements.Skip(1))
                ViewBoxDimensions = SVGPath.Union(ViewBoxDimensions, p.BoundingBox());
        }
        ViewBoxDimensions = ViewBoxDimensions.Inflate(margin);
        DocumentDimensions = new Coordinate(ViewBoxDimensions.Width, ViewBoxDimensions.Height);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.WriteLine(StartSvg);
        if (!string.IsNullOrWhiteSpace(InfoComment))
        {
            sw.WriteLine("<!--");
            sw.Write(InfoComment);
            sw.WriteLine("-->");
        }
        foreach (IRenderable element in svgElements)
            sw.WriteLine(element.ToString());
        sw.WriteLine(EndSvg);
        return sw.ToString();
    }
}
