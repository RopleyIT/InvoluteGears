using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoDimensionLib;

namespace InvoluteGears;

public class CutoutCurves : ICutouts
{
    public IList<DrawablePath> CalculateCutouts(IGearProfile gear)
    {
        IList<DrawablePath> cutoutCurves = new List<DrawablePath>();
        int spokes = SpokeCount(gear);
        if (spokes == 0)
            return cutoutCurves;

        // Set some design constants

        double cornerRadius = gear.Module;
        double spokeThickness = 2.0 * gear.Module;
        double minHubDiameter = 8 * gear.Module;

        // Calculate the minimum hub diameter for a given number of
        // spokes, a specified spoke thickness and corner radius.

        double hubDiameter = (spokeThickness + 2 * cornerRadius)
            / Math.Sin(Math.PI / spokes) - cornerRadius;
        if (hubDiameter < minHubDiameter)
            hubDiameter = minHubDiameter;

        // Calculate the corner at the outer end of one side of a spoke.
        // For this reference spoke we assume the spoke runs along the
        // positive X axis. We shall rotate it for other spokes.

        double rimDiameter = gear.InnerDiameter - 2.0 * spokeThickness;
        if (rimDiameter < hubDiameter + 4 * cornerRadius)
            return cutoutCurves;

        double cornerCentreY = spokeThickness / 2 + cornerRadius;
        double rimCornerCentreX = Math.Sqrt
            (Geometry.DiffOfSquares(rimDiameter / 2 - cornerRadius, cornerCentreY));
        Coordinate rimCornerCentre = new(rimCornerCentreX, cornerCentreY);
        double angleAtRim = Math.Atan2(cornerCentreY, rimCornerCentreX);
        IDrawable outerCornerCurve = new CircularArc
        {
            StartAngle = -Math.PI / 2,
            EndAngle = angleAtRim,
            Radius = cornerRadius,
            Centre = rimCornerCentre,
            Anticlockwise = true
        };

        // Calculate the corner at the inner end of a spoke.

        double hubCornerCentreX = Math.Sqrt
            (Geometry.DiffOfSquares(hubDiameter / 2 + cornerRadius, cornerCentreY));
        Coordinate hubCornerCentre = new(hubCornerCentreX, cornerCentreY);
        double angleAtHub = Math.Atan2(cornerCentreY, hubCornerCentreX);

        IDrawable innerCornerCurve = new CircularArc
        {
            StartAngle = Math.PI + angleAtHub,
            EndAngle = 1.5 * Math.PI,
            Radius = cornerRadius,
            Centre = hubCornerCentre,
            Anticlockwise = true
        };

        IDrawable outerRimCurve = new CircularArc
        {
            StartAngle = angleAtRim,
            EndAngle = 2 * Math.PI / spokes - angleAtRim,
            Radius = rimDiameter / 2,
            Centre = Coordinate.Empty,
            Anticlockwise = true
        };

        IDrawable hubCurve = new CircularArc
        {
            StartAngle = angleAtHub,
            EndAngle = 2 * Math.PI / spokes - angleAtHub,
            Radius = hubDiameter / 2,
            Centre = Coordinate.Empty,
            Anticlockwise = true
        };

        // Construct the first cutout curve from its
        // various bends and straight lines

        IList<IDrawable> cutoutCurve = new List<IDrawable>
        {
            innerCornerCurve,
            new Line
                (innerCornerCurve.End, outerCornerCurve.Start),
            outerCornerCurve,
            outerRimCurve,
            outerCornerCurve
                .ReflectY()
                .RotatedBy(2 * Math.PI / spokes, Coordinate.Empty)
                .Reversed()
        };
        IDrawable otherInnerCornerCurve = innerCornerCurve
            .ReflectY()
            .RotatedBy(2 * Math.PI / spokes, Coordinate.Empty)
            .Reversed();
        cutoutCurve.Add(new Line
            (cutoutCurve.Last().End, otherInnerCornerCurve.Start));
        cutoutCurve.Add(otherInnerCornerCurve);
        cutoutCurve.Add(hubCurve.Reversed());

        // Now add the cutout curve to the list of curves

        cutoutCurves.Add(new DrawablePath
        {
            Curves = cutoutCurve,
            Closed = true
        });

        // Repeat for each of the other spokes in the wheel

        for (int i = 1; i < spokes; i++)
        {
            IList<IDrawable> cutout = new List<IDrawable>();
            cutout.AddRange(cutoutCurve.Select(c => c.RotatedBy
                    (2 * Math.PI * i / spokes, Coordinate.Empty)));
            cutoutCurves.Add(new DrawablePath
            {
                Curves = cutout,
                Closed = true
            });
        }
        return cutoutCurves;
    }

    /// <summary>
    /// Calculate the number of spokes between the hub
    /// and the perimeter of the gear. Based on the number
    /// of teeth in the gear.
    /// </summary>
    /// <returns>The number of spokes to cut out</returns>

    private static int SpokeCount(IGearProfile gear)
    {
        int spokes = 1 + (gear.ToothCount - 1) / 8;
        return spokes >= 3 ? spokes : 0;
    }

}
