using Plotter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public static class GearGenerator
{
    public static void GenerateSVGFile(Cutouts cutoutCalculator, float docDimension, string file)
    {
        using StreamWriter sw = new($"{file}.svg");
        sw.Write(GenerateSVGCurves(cutoutCalculator, docDimension));
        sw.Close();
    }

    public static string GenerateSVGCurves(Cutouts cutoutCalculator, float docDimension)
    {
        SVGCreator svgCreator = new()
        {
            InfoComment = cutoutCalculator.Gear.Information + cutoutCalculator.Information
        };
        foreach (DrawablePath p in cutoutCalculator.Curves.Paths)
            svgCreator.AddPath(new SVGPath(p), "black", 0.1, "transparent").Join = LineJoin.Round;
        
        svgCreator.DocumentDimensions = new Coordinate(docDimension, docDimension);
        svgCreator.DocumentDimensionUnits = "mm";
        svgCreator.ViewBoxDimensions = new TwoDimensionLib.Rectangle(
            new(-svgCreator.DocumentDimensions.X / 2.0,
            -svgCreator.DocumentDimensions.Y / 2.0),
            svgCreator.DocumentDimensions.X, svgCreator.DocumentDimensions.Y);
        svgCreator.ViewBoxDimensionUnits = "";
        return svgCreator.ToString();
    }
}
