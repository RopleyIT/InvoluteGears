﻿using System;
using System.Collections.Generic;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

/// <summary>
/// Computes the cutouts of gear faces to reduce gear inertia
/// for larger gears. Computes the central cutouts for
/// bearings or spindles.
/// </summary>

public class Cutouts
{
    /// <summary>
    /// The gear this cutout is being applied to
    /// </summary>

    public IGearProfile Gear { get; private set; }

    /// <summary>
    /// The diameter of the hole right through the centre of the gear
    /// </summary>

    public double SpindleDiameter { get; private set; }

    /// <summary>
    /// The diameter of an optional inlaid bearing, that is
    /// inserted from one side of the gear. Set zero to
    /// not use this.
    /// </summary>

    public double InlayDiameter { get; private set; }

    /// <summary>
    /// The distance across the flats of the hexagonal
    /// key shape drawn on the cutout. This hex key is
    /// used to align adjacent gears where they are
    /// keyed together as a wheel-pinion pair.
    /// </summary>

    public double KeyWidth { get; private set; }

    /// <summary>
    /// Used for warning or information messages when methods invoked
    /// </summary>

    public string Information { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gear">The gear we wish to calculate cutouts for</param>
    /// <param name="spindle">The diameter of the central hole in the gear</param>
    /// <param name="inlay">The diameter of the bearing inlay</param>
    /// <param name="keyWidth">The distance across flats of the hex key for attaching
    /// gears to each other truly</param>

    public Cutouts(IGearProfile gear, double spindle, double inlay, double keyWidth)
    {
        if (spindle < 0 || inlay < 0 || keyWidth < 0)
            throw new ArgumentException("Dimensions cannot be negative");
        Gear = gear ?? throw new ArgumentException("No gear specified for cut out");
        SpindleDiameter = spindle;
        InlayDiameter = inlay;
        KeyWidth = keyWidth;

        // Make the calculations

        CutoutPlots = CalculateCutouts();
        if (SpindleDiameter > 0)
        {
            SpindlePlot = CalculateSpindle();
            Information += $"Spindle dia. = {SpindleDiameter}mm, ";
        }
        else
            Information += "No spindle, ";

        if (InlayDiameter > 0)
        {
            InlayPlot = CalculateInlay();
            Information += $"inlay dia. = {InlayDiameter}mm, ";
        }
        else
            Information += "no inlay, ";
        if (KeyWidth > 0)
        {
            HexKeyPlot = CalculateHexKey();
            Information += $"hex key width = {KeyWidth}mm\r\n";
        }
        else
            Information += "no hex key\r\n";
    }

    /// <summary>
    /// Add an extra cutout profile to the set of plots 
    /// in this shape. Used for chain sprockets for example
    /// where there is a recess groove as well as an outer shape.
    /// </summary>
    /// <param name="points">The contour to add to the plot</param>

    public void AddPlot(List<Coordinate> points)
    {
        CutoutPlots.Add(points);
    }

    /// <summary>
    /// The cutout shapes to be added to the gear point sets
    /// </summary>

    public IList<IList<Coordinate>> CutoutPlots { get; private set; }
    public IList<Coordinate> SpindlePlot { get; private set; }
    public IList<Coordinate> InlayPlot { get; private set; }
    public IList<Coordinate> HexKeyPlot { get; private set; }

    /// <summary>
    /// Calculate the points that form the spindle circle
    /// </summary>
    /// <returns>Sequence of points that make up the hole
    /// in the middle of the gear</returns>

    private IList<Coordinate> CalculateSpindle()
        => Geometry.LinearReduction(Geometry.CirclePoints
            (-Math.PI, Math.PI, Geometry.AngleStep, SpindleDiameter / 2).ToList(),
            (float)Gear.MaxError);


    /// <summary>
    /// Calculate the points that form the inlaid circle
    /// </summary>
    /// <returns>Sequence of points that make up the bearing 
    /// inlay in the middle of the gear</returns>

    private IList<Coordinate> CalculateInlay()
        => Geometry.LinearReduction(Geometry.CirclePoints
            (-Math.PI, Math.PI, Geometry.AngleStep, InlayDiameter / 2).ToList(),
            (float)Gear.MaxError);

    /// <summary>
    /// Calculate the sequence of cutout point sequences that make
    /// the spokes in the gear. Used to try and reduce the inertia
    /// of larger gears.
    /// </summary>
    /// <param name="spokes">The number of spokes to put between
    /// the gear hub and the teeth-bearing perimeter</param>
    /// <returns>The point lists that each make up the
    /// outline of the cutouts</returns>

    private IList<IList<Coordinate>> CalculateCutouts()
        => CalculateCutouts(1 + (Gear.ToothCount - 1) / 8);

    /// <summary>
    /// Calculate the sequence of cutout point sequences that make
    /// the spokes in the gear. Used to try and reduce the inertia
    /// of larger gears.
    /// </summary>
    /// <param name="spokes">The number of spokes to put between
    /// the gear hub and the teeth-bearing perimeter</param>
    /// <returns>The point lists that each make up the
    /// outline of the cutouts</returns>

    private IList<IList<Coordinate>> CalculateCutouts(int spokes)
    {
        List<IList<Coordinate>> cutouts = new();
        if (spokes < 3)
            return cutouts;

        // Set some design constants

        double cornerRadius = Gear.Module;
        double spokeThickness = 2.0 * Gear.Module;
        double minHubDiameter = 8 * Gear.Module;

        // Calculate the minimum hub diameter for a given number of
        // spokes, a specified spoke thickness and corner radius.

        double hubDiameter = (spokeThickness + 2 * cornerRadius)
            / Math.Sin(Math.PI / spokes) - cornerRadius;
        if (hubDiameter < minHubDiameter)
            hubDiameter = minHubDiameter;

        // Calculate the corner at the outer end of one side of a spoke.
        // For this reference spoke we assume the spoke runs along the
        // positive X axis. We shall rotate it for other spokes.

        double rimDiameter = Gear.InnerDiameter - 2.0 * spokeThickness;
        if (rimDiameter < hubDiameter + 4 * cornerRadius)
            return cutouts;

        double cornerCentreY = spokeThickness / 2 + cornerRadius;
        double rimCornerCentreX = Math.Sqrt
            (Coordinate.DiffOfSquares(rimDiameter / 2 - cornerRadius, cornerCentreY));
        Coordinate rimCornerCentre = new(rimCornerCentreX, cornerCentreY);
        double angleAtRim = Math.Atan2(cornerCentreY, rimCornerCentreX);
        IEnumerable<Coordinate> outerCorner = Geometry.CirclePoints
            (-Math.PI / 2, angleAtRim, Geometry.AngleStep, cornerRadius, rimCornerCentre);

        // Calculate the corner at the inner end of a spoke.

        //double hubCornerCentreX = Math.Sqrt(Square(hubDiameter / 2 + cornerRadius)
        //    - Square(cornerCentreY));
        double hubCornerCentreX = Math.Sqrt
            (Coordinate.DiffOfSquares(hubDiameter / 2 + cornerRadius, cornerCentreY));
        Coordinate hubCornerCentre = new(hubCornerCentreX, cornerCentreY);
        double angleAtHub = Math.Atan2(cornerCentreY, hubCornerCentreX);

        IEnumerable<Coordinate> innerCorner = Geometry.CirclePoints
            (Math.PI + angleAtHub, 3.0 * Math.PI / 2, Geometry.AngleStep, cornerRadius, hubCornerCentre);

        // Calculate the outer rim circle segment

        IEnumerable<Coordinate> outerRimSegment = Geometry.CirclePoints
            (angleAtRim, 2 * Math.PI / spokes - angleAtRim, Geometry.AngleStep, rimDiameter / 2);

        // Calculate the hub circle segment

        IEnumerable<Coordinate> hubSegment = Geometry.CirclePoints
            (angleAtHub, 2 * Math.PI / spokes - angleAtHub, Geometry.AngleStep, hubDiameter / 2);

        // Calculate the far side of the cutout. Reflect the inner spoke
        // across the X axis, and reverse its points. Then rotate it round the gear
        // by the angle between adjacent spokes. This gives us the correct
        // list of points.

        IEnumerable<Coordinate> nearSide = innerCorner.Concat(outerCorner);
        IEnumerable<Coordinate> farSide = GearParameters
            .ReflectY(nearSide)
            .Rotated(2 * Math.PI / spokes)
            .Reverse();

        // Now create the lists of points for each of the cut outs

        IList<Coordinate> cutout = new List<Coordinate>();
        cutout.AddRange(nearSide);
        cutout.AddRange(outerRimSegment);
        cutout.AddRange(farSide);
        cutout.AddRange(hubSegment.Reverse());
        cutout = Geometry.LinearReduction(cutout, (float)Gear.MaxError);
        cutouts.Add(cutout);
        for (int i = 1; i < spokes; i++)
            cutouts.Add(new List<Coordinate>(cutout.Rotated(2 * Math.PI * i / spokes)));
        return cutouts;
    }

    /// <summary>
    /// Create a hexagonal key shape
    /// around the inlay so that
    /// two gears can be married on
    /// the same centres correctly
    /// </summary>
    /// <returns>The list of points 
    /// corresponding to the hex key</returns>

    private List<Coordinate> CalculateHexKey()
    {
        // Generate the points for one sixth of the key
        double ctrToFace = KeyWidth / 2;
        Coordinate cornerCtr = new(
            ctrToFace - Gear.CutDiameter / 2,
            (ctrToFace - Gear.CutDiameter / 2) / Math.Sqrt(3.0));
        IList<Coordinate> firstSegment = new List<Coordinate>()
        {
            new Coordinate(ctrToFace, 0),
            new Coordinate(ctrToFace, cornerCtr.Y)
        };
        firstSegment.AddRange(
            Geometry.CirclePoints(0, Math.PI / 3, Geometry.AngleStep,
                Gear.CutDiameter / 2, cornerCtr));
        firstSegment.Add(new Coordinate(ctrToFace / 2, Math.Sin(Math.PI / 3) * ctrToFace));
        firstSegment = Geometry.LinearReduction(firstSegment, (float)Gear.MaxError);
        return Enumerable
            .Range(0, 6)
            .Select(i => firstSegment.Rotated(i * Math.PI / 3.0))
            .SelectMany(ep => ep)
            .ToList();
    }
}

