using System;
using System.Collections.Generic;
using TwoDimensionLib;

namespace InvoluteGears;

public class HoleCutoutKey : ICutouts
{
    private readonly double holeDiameter = 0;
    private readonly int holeCount = 0;
    public HoleCutoutKey(double hd, int holes)
    {
        holeDiameter = hd;
        holeCount = holes;
    }

    public IList<DrawablePath> CalculateCutouts(IGearProfile gear)
    {
        List<DrawablePath> paths = new();
        for (int i = 0; i < holeCount; i++)
        {
            paths.Add(CalculateHoleKeyCurve(gear, 2 * i * Math.PI / holeCount));
        }
        return paths;
    }

    /// <summary>
    /// Create a single dowel hole
    /// key shape so that
    /// two gears can be married on
    /// the same centres correctly
    /// </summary>
    /// <returns>The list of drawables 
    /// corresponding to the hex key</returns>

    private DrawablePath CalculateHoleKeyCurve(IGearProfile gear, double angle)
    {
        double radius = 3 * gear.Module;
        Coordinate holeCentre = new Coordinate(radius*Math.Cos(angle), radius*Math.Sin(angle));
        return CircularArc.Circle(holeDiameter / 2, holeCentre);
    }
}
