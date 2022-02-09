using System;
using System.Collections.Generic;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public class ChainSprocket : IGearProfile
{
    public ChainSprocket(int teeth, double wire, double err, double inner, double outer, double cutDiameter, double backlash)
    {
        ToothCount = teeth;
        WireThickness = wire;
        OuterLinkWidth = outer;
        MaxError = err;
        Backlash = backlash;
        InnerLinkLength = inner;
        CutDiameter = cutDiameter;
        SetInformation();
        var curves = CalculatePoints();
        OuterToothProfile = Geometry.LinearReduction
            (curves.outer, MaxError);
        InnerToothProfile = Geometry.LinearReduction
            (curves.inner, MaxError);
    }

    private void SetInformation()
    {
        Information = $"Sprocket: {ToothCount} teeth, link thickness = {WireThickness}mm\r\n";
        Information += $"precision = {MaxError}mm, inner length = {InnerLinkLength}mm\r\n";
        Information += $"outer width = {OuterLinkWidth}mm\r\n";
    }

    public string ShortName
        => $"St{ToothCount}w{WireThickness:N2}e{MaxError:N2}i{InnerLinkLength:N2}o{OuterLinkWidth:N2}.svg";

    public string Information { get; private set; }

    /// <summary>
    /// A count of the number of chain links around the sprocket
    /// divided by two. Note this is because we are counting the
    /// links that are parallel to the surface of the sprocket,
    /// not the alternate ones that are normal to it.
    /// </summary>

    public int ToothCount { get; private set; }

    /// <summary>
    /// The distance across the width of a chain link
    /// including the thickness of the link wire
    /// </summary>

    public double OuterLinkWidth { get; private set; }

    /// <summary>
    /// The length of the opening in the middle of a
    /// chain link, measured between the inner ends
    /// of a link
    /// </summary>

    public double InnerLinkLength { get; private set; }

    /// <summary>
    /// The diameter of the wire used to make each
    /// chain link, asssumed to be cylindrical
    /// </summary>

    public double WireThickness { get; private set; }

    /// <summary>
    /// The tolerance in precision for cutting the sprocket
    /// </summary>

    public double MaxError { get; private set; }

    /// <summary>
    /// Loosening of the sprocket to allow for
    /// variation in chain link dimensions.
    /// Manifests itself as longer grooves where
    /// the coplanar links bed into the edge of
    /// the sprocket.
    /// </summary>

    public double Backlash { get; private set; }

    /// <summary>
    /// The diameter of the cutter bit used to cut out
    /// the sprocket shape
    /// </summary>

    public double CutDiameter { get; private set; }

    /// <summary>
    /// Sprockets are not designed by setting their module.
    /// Instead they are designed from the dimensions of
    /// each chain link. However the module is used to compute
    /// the spoke thicknesses, so set a value notionally
    /// based on the diameter and number of chain links
    /// </summary>

    public double Module { get; private set; }

    private double radius = 0;

    /// <summary>
    /// Sprockets do not have a meaningful interpretation
    /// of an inner diameter. However, it is taken to be
    /// twice the distance from the centre of the sprocket
    /// to the nearest groove point on the X axis.
    /// </summary>

    public double InnerDiameter { get; private set; }

    private static double Sqr(double x) => x * x;

    private (List<Coordinate> outer, List<Coordinate> inner) CalculatePoints()
    {
        // Calculate the inside radius of one end of a link,
        // the two distances for the unequal faces of the
        // polygon that is the profile for the curvature of
        // the chiin links around the sprocket, and the angle
        // between two adjacent links in the chain

        double r = OuterLinkWidth / 2 - WireThickness;
        double a = InnerLinkLength + 2 * r;
        double b = InnerLinkLength - 2 * r;
        double C = Math.PI * (ToothCount - 1) / (double)ToothCount;

        // Use the cosine rule to find the distance between
        // adjacent pairs of links

        double lSquared = Sqr(a);
        lSquared += Sqr(b);
        lSquared -= 2 * a * b * Math.Cos(C);
        double l = Math.Sqrt(lSquared);

        // Find the other two angles, A and B at the other two
        // corners of the same triangle

        // double A = Math.Acos((lSquared + a * a - b * b) / (2 * l * a));
        // double B = Math.PI - A - C;

        // Now compute the radius from the centre of the
        // sprocket to one end of the line between adjacent
        // pairs of links

        radius = l / (2 * Math.Sin(Math.PI / ToothCount));

        // We shall set the origin at the centre of the sprocket. The
        // point in the middle of a link that is parallel to the
        // plane of the sprocket lies on the X axis, the link being
        // parallel to the Y axis. T is the centre of the semicircle
        // formed by one end of the link

        Coordinate t = new(
            Geometry.RootDiffOfSquares(radius, b / 2), b / 2);

        // C is the centre of the wire for the next link, lying on a
        // line from T in the direction of the 3rd link

        double sinTooth = Math.Sin(Math.PI / ToothCount);
        double cosTooth = Math.Cos(Math.PI / ToothCount);

        Coordinate c = new(
            t.X - (r - WireThickness / 2) * sinTooth,
            t.Y + (r - WireThickness / 2) * cosTooth);

        // U is the point on the second link parallel to the X
        // axis, closest to the centre of the first link.
        // V is the point on the second link just past its
        // curvature due to the wire thickness, where it
        // becomes parallel to the diagonal face of the sprocket
        // beyond the first link.

        Coordinate u = new(c.X, c.Y - WireThickness / 2);
        double arcAngle;
        double arcRadius = CutDiameter / 2; // Assume cutter bigger than wire
        Coordinate convexCtr;

        // Find length to mid point of face on which
        // perpendicular link lies

        double oz = Geometry
            .RootDiffOfSquares(radius, a / 2) - WireThickness / 2;
        Coordinate z = new(oz * cosTooth, oz * sinTooth);

        if (WireThickness >= CutDiameter)
        {
            arcRadius = WireThickness / 2;
            // v = Involutes.CreatePt(
            //    c.X - arcRadius * cosTooth,
            //    c.Y - arcRadius * sinTooth);
            arcAngle = Math.PI * (0.5 - 1.0 / ToothCount);
            convexCtr = c;
        }
        else
        {
            // Coordinate of cutter centre when cutting corner
            // between parallel and perpendicular links

            convexCtr = new(u.X, u.Y + arcRadius);

            // Find the distance from the cutter centre to the sloping face

            double cPerp = u.X * cosTooth - oz + sinTooth * (u.Y + arcRadius);

            // Find the angle from the perpendicular to the intersection point

            arcAngle = Math.Acos(cPerp / arcRadius)
                + Math.PI / 2 - Math.PI / ToothCount;
        }

        // Now plot one chain link worth of profile

        List<Coordinate> points = new()
        {
            new Coordinate(t.X + OuterLinkWidth / 2, 0)
        };
        Coordinate cUpper = new(t.X, u.Y - OuterLinkWidth / 2);
        points.AddRange(Geometry.CirclePoints(
            0, Math.PI / 2, Math.PI / 180, OuterLinkWidth / 2, cUpper));
        points.AddRange(Geometry.CirclePoints(
            1.5 * Math.PI - arcAngle, 1.5 * Math.PI, Math.PI / 180, arcRadius, convexCtr)
            .Reverse());
        points.Add(z);

        // Now calculate the recess profile

        double grooveStartAngle = Math.Acos(WireThickness / OuterLinkWidth);
        grooveStartAngle = Math.PI * (1.0 + ToothCount) / ToothCount
            - grooveStartAngle;

        Coordinate bt = new(t.X - Backlash * sinTooth, t.Y + Backlash * cosTooth);
        List<Coordinate> groovePoints = new();
        InnerDiameter = 2 * t.X - OuterLinkWidth;
        Module = InnerDiameter / ToothCount;
        groovePoints.Add(new Coordinate(InnerDiameter / 2 - Backlash * sinTooth, 0));
        groovePoints.AddRange(Geometry.CirclePoints(
            grooveStartAngle, Math.PI, Math.PI / 180, OuterLinkWidth / 2, bt)
            .Reverse());
        groovePoints.Add(z);
        return (points, groovePoints);
    }

    private readonly IList<Coordinate> OuterToothProfile = null;
    private readonly IList<Coordinate> InnerToothProfile = null;


    /// <summary>
    /// Generate the sequence of points describing the
    /// shape of a single tooth profile
    /// </summary>
    /// <param name="gap">Which tooth profile we want.
    /// For gap = 0, we generate the tooth whose
    /// tip lies on the positive X axis. Teeth rotate
    /// anticlockwise from there for increasing
    /// values of gap.</param>
    /// <returns>The set of points describing the
    /// profile of the selected tooth.</returns>

    private IEnumerable<Coordinate> ToothProfile(int gap, bool outer)
    {
        IList<Coordinate> profile = outer ? OuterToothProfile : InnerToothProfile;
        double angle = 2 * Math.PI * (gap % ToothCount) / (double)ToothCount;
        return
            profile
            .Skip(1)
            .Reverse()
            //.Skip(1)
            .Select(p => p.Conjugate)
            .Concat(profile)
            .Select(p => p.Rotate(angle));
    }

    /// <summary>
    /// Generate the complete path of
    /// points for the whole sprocket outer edge
    /// </summary>
    /// <returns>The set of points describing the sprocket outer edge
    /// </returns>

    public IEnumerable<Coordinate> GenerateCompleteGearPath() => Enumerable
            .Range(0, ToothCount)
            .Select(i => ToothProfile(i, true))
            .SelectMany(p => p);

    /// <summary>
    /// Generate the complete path of
    /// points for the whole sprocket inner groove
    /// </summary>
    /// <returns>The set of points describing the sprocket groove
    /// </returns>

    public IEnumerable<Coordinate> GenerateInnerGearPath() => Enumerable
            .Range(0, ToothCount)
            .Select(i => ToothProfile(i, false))
            .SelectMany(p => p);
}
