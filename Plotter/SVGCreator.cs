using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Plotter;

// TODO: Modify to take multiple paths in format that can be fed to Cut2D

/*
 <?xml version="1.0" encoding="UTF-8" standalone="no"?>
    <svg width="391" height="391" viewBox="-70.5 -70.5 391 391" xmlns="http://www.w3.org/2000/svg">
    <rect fill="#fff" stroke="#000" x="-70" y="-70" width="390" height="390"/>
    <g opacity="0.8">
        <rect x="25" y="25" width="200" height="200" fill="green" stroke-width="4" stroke="pink" />
        <circle cx="125" cy="125" r="75" fill="orange" />
        <polyline points="50,150 50,200 200,200 200,100" stroke="red" stroke-width="4" fill="none" />
        <line x1="50" y1="50" x2="200" y2="200" stroke="blue" stroke-width="4" />
    </g>
    </svg>
 */
public class SVGCreator
{
    private readonly List<SVGPath> svgPaths;

    public SVGCreator() => svgPaths = new List<SVGPath>();

    public void AddPath(SVGPath path) => svgPaths.Add(path);

    public void AddClosedPath(IEnumerable<PointF> points, string stroke, double strokeWidth, string fill)
    {
        SVGPath path = new(points, true);
        path.SetDrawingParams(stroke, strokeWidth, fill);
        AddPath(path);
    }

    public SizeF DocumentDimensions { get; set; } = SizeF.Empty;

    public string DocumentDimensionUnits { get; set; } = string.Empty;

    public RectangleF ViewBoxDimensions { get; set; } = RectangleF.Empty;

    public string ViewBoxDimensionUnits { get; set; } = string.Empty;

    public string InfoComment { get; set; } = string.Empty;

    private static string XmlHeader => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>";

    private string StartSvg =>
        $"<svg width=\"{DocumentDimensions.Width}{DocumentDimensionUnits}\" "
        + $"height=\"{DocumentDimensions.Height}{DocumentDimensionUnits}\" "
        + $"viewBox=\"{ViewBoxDimensions.X}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Y}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Width}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Height}{ViewBoxDimensionUnits}\" "
        + $"xmlns=\"http://www.w3.org/2000/svg\">";

    private static string EndSvg => "</svg>";

    public void CalculateViewBox()
    {
        if (svgPaths != null && svgPaths.Count > 0)
        {
            ViewBoxDimensions = svgPaths[0].BoundingBox();
            foreach (SVGPath p in svgPaths.Skip(1))
                ViewBoxDimensions = SVGPath.Union(ViewBoxDimensions, p.BoundingBox());
        }
        DocumentDimensions = new SizeF(ViewBoxDimensions.Width, ViewBoxDimensions.Height);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.WriteLine(XmlHeader);
        if (!string.IsNullOrWhiteSpace(InfoComment))
        {
            sw.WriteLine("<!--");
            sw.Write(InfoComment);
            sw.WriteLine("-->");
        }
        sw.WriteLine(StartSvg);
        foreach (SVGPath path in svgPaths)
            sw.WriteLine(path.ToString());
        sw.WriteLine(EndSvg);
        return sw.ToString();
    }
}
