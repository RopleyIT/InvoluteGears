using System;
using System.Collections.Generic;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public class Ratchet : IGearProfile
{
    public Ratchet(int teeth, double module, double err, double inner, double cutDiameter)
    {
        ToothCount = teeth;
        Module = module;
        MaxError = err;
        InnerDiameter = inner;
        CutDiameter = cutDiameter;
        SetInformation();
        CalculatePoints();
    }

    private void SetInformation()
    {
        Information = $"Ratchet: {ToothCount} teeth, module = {Module}mm\r\n";
        Information += $"precision = {MaxError}mm, inner diameter = {InnerDiameter}mm\r\n";
    }

    public string ShortName
        => $"Rt{ToothCount}m{Module:N2}e{MaxError:N2}i{InnerDiameter:N2}.svg";

    public string Information { get; private set; }

    public int ToothCount { get; private set; }
    public double Module { get; private set; }

    public double MaxError { get; private set; }

    public double InnerDiameter { get; private set; }

    public double CutDiameter { get; private set; }

    /// <summary>
    /// The angle occupied by one tooth and one gap
    /// </summary>

    public double ToothAngle => 2 * Math.PI / ToothCount;

    /// <summary>
    /// The pitch circle diameter for an escape
    /// wheel is its true diameter to the ends of
    /// the teeth.
    /// </summary>

    public double PitchCircleDiameter => Module * ToothCount;

    private double ToothDepth => (PitchCircleDiameter - InnerDiameter) / 2;

    private IList<Coordinate> OneToothProfile;

    private void CalculatePoints()
    {
        List<Coordinate> points = new();
        double innerRadius = InnerDiameter / 2;
        double cutterRadius = CutDiameter / 2;

        // Calculate the cutter centre for the tooth
        // of the ratchet gear at the innermost end

        double m = -ToothDepth / (innerRadius * ToothAngle);
        double xs = Math.Sqrt(cutterRadius * cutterRadius / (1 + m * m));
        double ys = m * xs;
        Coordinate cutterCentre = new(innerRadius + xs, ys);

        // Find the start angle and the end angle for the curve. The curve starts
        // at the X axis and heads towards negative Y. The end angle is where the
        // curve becomes tangential to a radial from the centre of the ratchet.
        // If the cutter centre lies outside the pitch circle, then the ratchet
        // will not be able to have a tooth at right angles to the direction
        // of rotation, and the ratchet is able to skip teeth under high torque.
        // Nonetheless, we calculate the tooth shape and return a warning.

        double radiusOfCutterCentre = cutterCentre.Magnitude;
        double cutterCentreAngle = Math.Asin(-ys / radiusOfCutterCentre);
        double tangentAngle = cutterCentreAngle
            + Math.Asin(cutterRadius / radiusOfCutterCentre);

        double endAngle = Math.PI - Math.Atan2(-ys, xs);
        double startAngle = 3 * Math.PI / 2 - tangentAngle;
        points.Add(PointAtAngle(ToothAngle - tangentAngle).Rotate(-ToothAngle));

        // Add the points for the cutter curve

        points.AddRange(Geometry.CirclePoints
            (endAngle, startAngle, Geometry.AngleStep, cutterRadius, cutterCentre)
            .Reverse());

        // Check that the radius of the cutter was not too great to
        // provide a latch at right angles to the radial from the centre
        // of the ratchet.

        double actualInnerRadius = cutterCentre.Magnitude - cutterRadius;
        double actualOuterRadius = points[0].Magnitude;
        if (points[1].Magnitude > actualOuterRadius)
        {
            Information += "Cutter diameter too great for locking ratchet. Setting it to zero.\r\n";
            CutDiameter = 0;
            OneToothProfile = new List<Coordinate>();
            return;
        }

        // Now add the slope

        for (double angle = 0; angle < ToothAngle - tangentAngle; angle += Geometry.AngleStep)
            points.Add(PointAtAngle(angle));
        OneToothProfile = Geometry.LinearReduction(points, (float)MaxError);
        Information += $"Actual inner diameter: {2 * actualInnerRadius:N2}, "
            + $"actual outer diameter: {2 * actualOuterRadius:N2}\r\n";
    }

    private Coordinate PointAtAngle(double angle)
    {
        double radius = InnerDiameter / 2
            + (PitchCircleDiameter - InnerDiameter) / 2
            * angle / ToothAngle;
        return Coordinate.FromPolar(radius, angle);
    }

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

    public IEnumerable<Coordinate> ToothProfile(int gap)
        => OneToothProfile.Rotated((gap % ToothCount) * ToothAngle);

    /// <summary>
    /// Generate the complete path of
    /// points for the whole  escape wheel
    /// </summary>
    /// <returns>The set of points describing the escape wheel
    /// </returns>

    public IEnumerable<Coordinate> GenerateCompleteGearPath() => Enumerable
            .Range(0, ToothCount)
            .Select(i => ToothProfile(i))
            .SelectMany(p => p);
}
