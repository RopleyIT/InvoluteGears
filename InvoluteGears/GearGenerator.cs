using Plotter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public static class GearGenerator
{
    //public static PointF FromCoord(Coordinate c)
    //    => new((float)c.X, (float)c.Y);

    //public static IEnumerable<PointF> FromCoords(this IEnumerable<Coordinate> coords)
    //    => coords.Select(c => FromCoord(c));

    //public static IEnumerable<IEnumerable<PointF>> FromCoordLists(this IEnumerable<IEnumerable<Coordinate>> coordLists)
    //    => coordLists.Select(cl => FromCoords(cl));

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
        var gearPath = cutoutCalculator.Gear.GenerateCompleteGearPath();
        if(gearPath.Any())
            svgCreator.AddPath(gearPath, true, string.Empty, 0, "black");
        if(cutoutCalculator.CutoutPlots != null)
            foreach (List<Coordinate> cutout in cutoutCalculator.CutoutPlots)
                svgCreator.AddPath(cutout, true, string.Empty, 0, "white");
        if (cutoutCalculator.HexKeyPlot != null)
            svgCreator.AddPath(cutoutCalculator.HexKeyPlot, true, string.Empty, 0, "lightgray");
        if (cutoutCalculator.InlayPlot != null)
            svgCreator.AddPath(cutoutCalculator.InlayPlot, true, string.Empty, 0, "gray");
        if (cutoutCalculator.SpindlePlot != null)
            svgCreator.AddPath(cutoutCalculator.SpindlePlot, true, string.Empty, 0, "white");

        svgCreator.DocumentDimensions = new Coordinate(docDimension, docDimension);
        svgCreator.DocumentDimensionUnits = "mm";
        svgCreator.ViewBoxDimensions = new TwoDimensionLib.Rectangle(
            new(-svgCreator.DocumentDimensions.X / 2.0,
            -svgCreator.DocumentDimensions.Y / 2.0),
            svgCreator.DocumentDimensions.X, svgCreator.DocumentDimensions.Y);
        svgCreator.ViewBoxDimensionUnits = "";
        return svgCreator.ToString();
    }

    public static void GenerateCutoutPlot(Cutouts cutoutCalculator, List<IEnumerable<Coordinate>> gearPoints)
    {
        if(cutoutCalculator.CutoutPlots != null)
            foreach (var cutout in cutoutCalculator.CutoutPlots)
                gearPoints.Add(cutout);
        if (cutoutCalculator.HexKeyPlot != null)
            gearPoints.Add(cutoutCalculator.HexKeyPlot);
        if (cutoutCalculator.InlayPlot != null)
            gearPoints.Add(cutoutCalculator.InlayPlot);
        if (cutoutCalculator.SpindlePlot != null)
            gearPoints.Add(cutoutCalculator.SpindlePlot);

    }
}
