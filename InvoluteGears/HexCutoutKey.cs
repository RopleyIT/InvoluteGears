using System;
using System.Collections.Generic;
using TwoDimensionLib;

namespace InvoluteGears;

public class HexCutoutKey : ICutouts
{
    private readonly double keyWidth = 0;

    public HexCutoutKey(double kw) => keyWidth = kw;

    public IList<DrawablePath> CalculateCutouts(IGearProfile gear) 
        => new List<DrawablePath>
        {
            CalculateHexKeyCurve(gear)
        };

    /// <summary>
    /// Create a hexagonal key shape
    /// around the inlay so that
    /// two gears can be married on
    /// the same centres correctly
    /// </summary>
    /// <returns>The list of drawables 
    /// corresponding to the hex key</returns>

    private DrawablePath CalculateHexKeyCurve(IGearProfile gear)
    {
        // Generate the points for one sixth of the key
        double ctrToFace = keyWidth / 2;
        Coordinate cornerCtr = new(
            ctrToFace - gear.CutDiameter / 2,
            (ctrToFace - gear.CutDiameter / 2) / Math.Sqrt(3.0));
        IList<IDrawable> keyCurves = new List<IDrawable>()
    {
        new Line(new Coordinate(ctrToFace, 0),
            new Coordinate(ctrToFace, cornerCtr.Y)),
        new CircularArc
        {
            StartAngle = 0,
            EndAngle = Math.PI / 3,
            Radius = gear.CutDiameter / 2,
            Centre = cornerCtr,
            Anticlockwise = true
        }
    };
        for (int i = 1; i <= 5; i++)
        {
            keyCurves.Add(keyCurves[0]
                .RotatedBy(i * Math.PI / 3.0, Coordinate.Empty));
            keyCurves.Add(keyCurves[1]
                .RotatedBy(i * Math.PI / 3.0, Coordinate.Empty));
        }
        return new DrawablePath
        {
            Curves = keyCurves,
            Closed = true
        };
    }
}
