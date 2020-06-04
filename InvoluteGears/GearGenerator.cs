using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Plotter;
using System.IO;

namespace InvoluteGears
{
    public static class GearGenerator
    {
        public static void GenerateSVGFile(Cutouts cutoutCalculator, float docDimension, string file)
        {
            using StreamWriter sw = new StreamWriter($"{file}.svg");
            sw.Write(GenerateSVG(cutoutCalculator, docDimension));
            sw.Close();
        }

        public static string GenerateSVG(Cutouts cutoutCalculator, float docDimension)
        {
            SVGCreator svgCreator = new SVGCreator();
            svgCreator.AddClosedPath(cutoutCalculator.Gear.GenerateCompleteGearPath(), string.Empty, 0, "black");
            foreach (List<PointF> cutout in cutoutCalculator.CutoutPlots)
                svgCreator.AddClosedPath(cutout, string.Empty, 0, "white");
            if (cutoutCalculator.HexKeyPlot != null)
                svgCreator.AddClosedPath(cutoutCalculator.HexKeyPlot, string.Empty, 0, "lightgray");
            if (cutoutCalculator.InlayPlot != null)
                svgCreator.AddClosedPath(cutoutCalculator.InlayPlot, string.Empty, 0, "gray");
            if (cutoutCalculator.SpindlePlot != null)
                svgCreator.AddClosedPath(cutoutCalculator.SpindlePlot, string.Empty, 0, "white");

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
                foreach (List<PointF> cutout in cutoutCalculator.CutoutPlots)
                    gearPoints.Add(cutout);
        }
    }
}
