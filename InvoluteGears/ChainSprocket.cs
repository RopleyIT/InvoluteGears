﻿using System;
using System.Collections.Generic;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears;

public class ChainSprocket : IGearProfile
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="teeth">Number of teeth around the perimeter. Note that this
    /// is half the number of chain links as the teeth only engage with the
    /// links that are normal to the plane of the sprocket.</param>
    /// <param name="wire">The diameter of the wire used to make the links</param>
    /// <param name="inner">The internal length of each link</param>
    /// <param name="outer">The external width of each link</param>
    /// <param name="cutDiameter">The diameter of the end mill used to
    /// cut out the sprocket. Limits the curvature of concave corners.</param>
    /// <param name="backlash">The amount of backlash to build into the
    /// sprocket for machining errors, expansion, etc.</param>
    
    public ChainSprocket(int teeth, double wire, double inner, double outer, double cutDiameter, double backlash)
    {
        ToothCount = teeth;
        WireThickness = wire;
        OuterLinkWidth = outer;
        Backlash = backlash;
        InnerLinkLength = inner;
        CutDiameter = cutDiameter;
        Errors = String.Empty;
        Information = SetInformation();
    }

    private string SetInformation()
    {
        var info = $"Sprocket: {ToothCount} teeth, link thickness = {WireThickness}mm\r\n";
        info += $"inside link length = {InnerLinkLength}mm\r\n";
        info += $"outside link width = {OuterLinkWidth}mm, backlash = {Backlash}mm\r\n";
        return info;
    }

    /// <summary>
    /// A unique name for the gear used for output filenames and similar purposes
    /// </summary>
    
    public string ShortName
        => $"St{ToothCount}w{WireThickness:N2}i{InnerLinkLength:N2}o{OuterLinkWidth:N2}b{Backlash:N2}";

    /// <summary>
    /// The less succinct description of the sprocket and all its parameters. Used on
    /// web pages and as comments in the output SVG files.
    /// </summary>
    
    public string Information { get; init; }

    /// <summary>
    /// Error messages generated by the gear generators. If an
    /// empty string, the gear generation has succeeded and the
    /// other fields will contain data. If non-empty the other
    /// fields may be empty or null.
    /// </summary>

    public string Errors { get; private set; }

    /// <summary>
    /// A count of the number of chain links around the sprocket
    /// divided by two. Note this is because we are counting the
    /// links that are normal to the surface of the sprocket,
    /// not the alternate ones that are coplanar to it.
    /// </summary>

    public int ToothCount { get; private set; }

    /// <summary>
    /// The angle between adjacent teeth at the centre of the gear
    /// </summary>
    
    public double ToothAngle
        => 2 * Math.PI / ToothCount;

    /// <summary>
    /// The distance across the width of a chain link
    /// including the thickness of the link wire
    /// </summary>

    public double OuterLinkWidth { get; private set; }

    /// <summary>
    /// The length of the opening in the middle of a
    /// chain link, measured between the inner ends
    /// of a link. Links are assumed to be made from
    /// two semicircular ends joined by lengths of
    /// parallel straight sides.
    /// </summary>

    public double InnerLinkLength { get; private set; }

    /// <summary>
    /// The diameter of the wire used to make each
    /// chain link, asssumed to be cylindrical
    /// </summary>

    public double WireThickness { get; private set; }

    /// <summary>
    /// The tolerance in precision for cutting the sprocket.
    /// This property is unused for SVG files, but kept
    /// here temporarily while it appears in the common
    /// interface IGearProfile.
    /// </summary>

    public double MaxError { get; private set; }

    /// <summary>
    /// Loosening of the sprocket to allow for
    /// variation in chain link dimensions.
    /// Manifests itself as longer grooves where
    /// the coplanar links bed into the edge of
    /// the sprocket, and narrower teeth to allow
    /// links to wind and unwind without snagging.
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
    /// the spoke thicknesses, so a value is set notionally
    /// based on the diameter and number of chain links
    /// </summary>

    public double Module { get; private set; }

    /// <summary>
    /// Sprockets do not have a meaningful interpretation
    /// of an inner diameter. However, it is taken to be
    /// twice the distance from the centre of the sprocket
    /// to the nearest groove point on the X axis.
    /// </summary>

    public double InnerDiameter { get; private set; }

    /// <summary>
    /// For a chain sprocket, this is taken to be the distance
    /// from the sprocket centre to the intersection of the line
    /// of centres between two adjacent chain links
    /// </summary>

    public double PitchRadius { get; private set; }

    public DrawableSet GenerateGearCurves() =>
        new()
        {
            Paths = new List<DrawablePath>
            {
                RimProfile(),
                PinProfile(),
                ShoulderProfile()
            }
        };

    // A chain has wire links made of two semicircles joined by two
    // parallel straight wires from their tips. The inside length
    // of the links is InnerLinkLength. The outside width of a link
    // across the parallel sides is OuterLinkWidth. The diameter of
    // the wire is WireThickness. The number of links around each
    // sprocket is 2*ToothCount, as alternate links are normal
    // then coplanar with the plane of the sprocket.
    //
    // We assume a normal link is pulled on the sprocket so that 
    // the plane in which it lies has a centre of curvature in the
    // middle of the wire at the end of the link.

    double n;
    double m;
    double cosTooth;
    double sinTooth;
    double normalRadius;

    /// <summary>
    /// Generate the closed path for the wheel rim
    /// at layers 1 and 5 of the chain sprocket. The
    /// diameter is chosen to be the pitch radius
    /// plus half the outside width of each link.
    /// This ensures links are completely contained
    /// within the rim.
    /// </summary>
    /// <returns>The path for the circular rime</returns>

    private DrawablePath RimProfile()
    {
        n = (InnerLinkLength + WireThickness) / 2;
        m = (InnerLinkLength - WireThickness) / 2;
        cosTooth = Math.Cos(ToothAngle / 2);
        sinTooth = Math.Sin(ToothAngle / 2);
        normalRadius = (m + n * cosTooth) / sinTooth;
        PitchRadius = Geometry.RootSumOfSquares(normalRadius, n);
        double rimRadius = PitchRadius + OuterLinkWidth / 2;
        return CircularArc.Circle(rimRadius);
    }

    /// <summary>
    /// Generate an entire gear path for the
    /// shoulder part of the chain sprocket,
    /// which forms layers 2 and 4 of the gear
    /// </summary>
    /// <returns>A drawable closed path for the
    /// pin part of the sprocket</returns>

    private DrawablePath ShoulderProfile()
    {
        IList<IDrawable> oneShoulder = OneShoulderProfile();
        DrawablePath profile = new ()
        {
            Closed = true,
            Curves = new List<IDrawable>()
        };
        for (int i = 0; i < ToothCount; i++)
        {
            profile.Curves.AddRange
                (oneShoulder.Select(d => d.RotatedBy
                    (i * ToothAngle, Coordinate.Empty)));
        }
        return profile;
    }

    /// <summary>
    /// Compute the shape of the first shoulder profile. A gear
    /// is made up of an outer rim at layers 1 and 5, shoulders
    /// at layers 2 and 4, and pins at layer 3.
    /// </summary>
    /// <returns>The template for the first shoulder. Should
    /// be replicated every 2 Pi / ToothCount around the 
    /// circumference. Note that ToothCount is half the number
    /// of links around the gear as there is a tooth every
    /// alternate link.
    /// </returns>

    private IList<IDrawable> OneShoulderProfile()
    {
        List<IDrawable> profile = new(4);
        Coordinate A = new(normalRadius, n);
        double BA =
            OuterLinkWidth / 2 - 3 * WireThickness / 2;
        Coordinate B = A + Coordinate.FromPolar
            (BA, Math.PI / 2 + ToothAngle / 2);
        double BE = BA * sinTooth - WireThickness / 2;
        Coordinate C = B + new Coordinate
        {
            X = BE,
            Y = -Geometry.RootDiffOfSquares((OuterLinkWidth + Backlash) / 2, BE)
        };
        profile.Add(new Line
        {
            Start = C.Conjugate,
            End = C
        });
        double sa = -Math.PI / 2 + Math.Asin(BE * 2 / (OuterLinkWidth + Backlash));
        double ea = -Math.PI + ToothAngle / 2;
        profile.Add(new CircularArc
        {
            Anticlockwise = false,
            Centre = B,
            Radius = (OuterLinkWidth + Backlash) / 2,
            StartAngle = sa,
            EndAngle = ea
        });
        Coordinate F = B + Coordinate.FromPolar
            ((OuterLinkWidth + Backlash) / 2, -Math.PI + ToothAngle / 2);
        Coordinate vecFG = Coordinate.FromPolar
            (InnerLinkLength + 2 * WireThickness - OuterLinkWidth,
            Math.PI / 2 + ToothAngle / 2);
        profile.Add(new Line
        {
            Start = F,
            End = F + vecFG
        });
        profile.Add(new CircularArc
        {
            Anticlockwise = false,
            Centre = B + vecFG,
            Radius = (OuterLinkWidth + Backlash) / 2,
            StartAngle = ea,
            EndAngle = 2 * ea - sa
        });
        return profile;
    }

    /// <summary>
    /// Generate an entire gear path for the
    /// pin part of the chain sprocket. This
    /// is the middle (3rd) layer of the gear.
    /// </summary>
    /// <returns>A drawable closed path for the
    /// pin part of the sprocket</returns>

    private DrawablePath PinProfile()
    {
        IList<IDrawable> onePin = OnePinProfile();
        DrawablePath profile = new ()
        {
            Closed = true,
            Curves = new List<IDrawable>()
        };
        for (int i = 0; i < ToothCount; i++)
        {
            profile.Curves.AddRange
                (onePin.Select(d => d.RotatedBy
                    (i * ToothAngle, Coordinate.Empty)));
        }
        return profile;
    }

    /// <summary>
    /// Compute the shape of the middle pin gear profile. A gear
    /// is made up of an outer rim at layers 1 and 5, shoulders
    /// at layers 2 and 4, and pins at layer 3.
    /// </summary>
    /// <returns>The template for the middle pin wheel. Should
    /// be replicated every 4 Pi / N around the circumference
    /// </returns>

    private IList<IDrawable> OnePinProfile()
    {
        List<IDrawable> profile = new(4);
        Coordinate A = new(normalRadius, n);
        double BA =
            OuterLinkWidth / 2 - 3 * WireThickness / 2;
        Coordinate B = A + Coordinate.FromPolar
            (BA, Math.PI / 2 + ToothAngle / 2);
        double BE = BA * sinTooth - WireThickness / 2;
        Coordinate C = B + new Coordinate
        {
            X = BE,
            Y = -Geometry.RootDiffOfSquares((OuterLinkWidth + Backlash) / 2, BE)
        };
        profile.Add(new Line
        {
            Start = C.Conjugate,
            End = C.Conjugate + Coordinate.FromPolar(WireThickness / 2, 0)
        });
        double radius = -InnerLinkLength / 2 - WireThickness / 2 + C.Y;
        double cornerAngle = Math.Atan2(n, normalRadius);

        // The pin or tooth should just glide inside the normal
        // link as the chain wraps/unwraps from the wheel. It is
        // assumed that the links slide frictionlessly in the
        // loop at the end of each link. Backlash should be set
        // to accommodate the tighter turning angle caused by
        // friction. The pin is shaped of a sequence of circular
        // segments that get a larger radius and a different
        // centre of curvature as each successive link begins
        // to unwrap from the gear.

        bool done = false;
        Coordinate curveCtr;
        for (int i = 0; !done; i++)
        {
            double tsa = i * ToothAngle / 2 - Math.PI / 2;
            double tea = tsa + ToothAngle / 2;
            bool odd = (i & 1) != 0;
            if (odd)
            {
                radius += InnerLinkLength - WireThickness;
                curveCtr = Coordinate.FromPolar
                    (PitchRadius, (i + 1) * ToothAngle / 2 - cornerAngle);
            }
            else
            {
                radius += InnerLinkLength + WireThickness;
                curveCtr = Coordinate.FromPolar
                    (PitchRadius, i * ToothAngle / 2 + cornerAngle);
            }

            // Check to see if the end of the compound tooth arc
            // lies in this curve segment. We calculate the angle
            // from the current curve centre to the Y=0 axis.

            double crossingAngle = Math.Asin(-curveCtr.Y / radius);
            if (crossingAngle < tea)
            {
                done = true;
                tea = crossingAngle;
            }

            profile.Add(new CircularArc
            {
                Anticlockwise = true,
                Centre = curveCtr,
                StartAngle = tsa,
                EndAngle = tea,
                Radius = radius
            });
        }

        // Construct the other side of the tooth

        IList<IDrawable> otherSideOfTooth = profile
            .AsEnumerable()
            .Reverse()
            .Select(d => d.ReflectY().Reversed())
            .ToList();
        profile.AddRange(otherSideOfTooth);

        double sa = -Math.PI / 2 + Math.Asin(BE * 2 / (OuterLinkWidth + Backlash));
        double ea = -Math.PI + ToothAngle / 2;
        profile.Add(new CircularArc
        {
            Anticlockwise = false,
            Centre = B,
            Radius = (OuterLinkWidth + Backlash) / 2,
            StartAngle = sa,
            EndAngle = ea
        });
        Coordinate F = B + Coordinate.FromPolar
            ((OuterLinkWidth + Backlash) / 2, -Math.PI + ToothAngle / 2);
        Coordinate vecFG = Coordinate.FromPolar
            (InnerLinkLength + 2 * WireThickness - OuterLinkWidth,
            Math.PI / 2 + ToothAngle / 2);
        profile.Add(new Line
        {
            Start = F,
            End = F + vecFG
        });
        Coordinate K = B + vecFG;
        profile.Add(new CircularArc
        {
            Anticlockwise = false,
            Centre = B + vecFG,
            Radius = (OuterLinkWidth + Backlash) / 2,
            StartAngle = ea,
            EndAngle = 2 * ea - sa
        });
        return profile;
    }
}
