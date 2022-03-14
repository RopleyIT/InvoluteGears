using Plotter;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public static class GearGenerator
{
    public static PointF FromCoord(Coordinate c)
        => new((float)c.X, (float)c.Y);

    public static IEnumerable<PointF> FromCoords(this IEnumerable<Coordinate> coords)
        => coords.Select(c => FromCoord(c));

    public static IEnumerable<IEnumerable<PointF>> FromCoordLists(this IEnumerable<IEnumerable<Coordinate>> coordLists)
        => coordLists.Select(cl => FromCoords(cl));

    public static void GenerateSVGFile(Cutouts cutoutCalculator, float docDimension, string file)
    {
        using StreamWriter sw = new($"{file}.svg");
        sw.Write(GenerateSVG(cutoutCalculator, docDimension));
        sw.Close();
    }

    public static string GenerateSVG(Cutouts cutoutCalculator, float docDimension)
    {
        SVGCreator svgCreator = new()
        {
            InfoComment = cutoutCalculator.Gear.Information + cutoutCalculator.Information
        };
        var gearPath = cutoutCalculator.Gear
            .GenerateCompleteGearPath()
            .FromCoords();
        if(gearPath.Any())
            svgCreator.AddClosedPath(gearPath, string.Empty, 0, "black");
        if(cutoutCalculator.CutoutPlots != null)
            foreach (List<Coordinate> cutout in cutoutCalculator.CutoutPlots)
                svgCreator.AddClosedPath(cutout.FromCoords(), string.Empty, 0, "white");
        if (cutoutCalculator.HexKeyPlot != null)
            svgCreator.AddClosedPath(cutoutCalculator.HexKeyPlot.FromCoords(), string.Empty, 0, "lightgray");
        if (cutoutCalculator.InlayPlot != null)
            svgCreator.AddClosedPath(cutoutCalculator.InlayPlot.FromCoords(), string.Empty, 0, "gray");
        if (cutoutCalculator.SpindlePlot != null)
            svgCreator.AddClosedPath(cutoutCalculator.SpindlePlot.FromCoords(), string.Empty, 0, "white");

        svgCreator.DocumentDimensions = new SizeF(docDimension, docDimension);
        svgCreator.DocumentDimensionUnits = "mm";
        svgCreator.ViewBoxDimensions = new RectangleF(
            -svgCreator.DocumentDimensions.Width / 2f,
            -svgCreator.DocumentDimensions.Width / 2f,
            svgCreator.DocumentDimensions.Width, svgCreator.DocumentDimensions.Height);
        svgCreator.ViewBoxDimensionUnits = "";
        return svgCreator.ToString();
    }

    public static void GenerateCutoutPlot(Cutouts cutoutCalculator, List<IEnumerable<PointF>> gearPoints)
    {
        if(cutoutCalculator.CutoutPlots != null)
            foreach (var cutout in cutoutCalculator.CutoutPlots)
                gearPoints.Add(cutout.FromCoords());
        if (cutoutCalculator.HexKeyPlot != null)
            gearPoints.Add(cutoutCalculator.HexKeyPlot.FromCoords());
        if (cutoutCalculator.InlayPlot != null)
            gearPoints.Add(cutoutCalculator.InlayPlot.FromCoords());
        if (cutoutCalculator.SpindlePlot != null)
            gearPoints.Add(cutoutCalculator.SpindlePlot.FromCoords());

    }
}
