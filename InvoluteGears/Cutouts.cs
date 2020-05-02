using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
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

        public GearParameters Gear { get; private set; }

        /// <summary>
        /// Flat raw material thickness from which
        /// the gear is cut
        /// </summary>

        public double Thickness { get; private set; }

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
        /// The depth of the inlay hole for the thickness
        /// of the bearing. Adjust as appropriate to
        /// position the bearing flush with the material
        /// surface, or central in the material.
        /// </summary>

        public double InlayDepth { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gear">The gear we wish to calculate cutouts for</param>
        /// <param name="thickness">The thickness of the material to be cut</param>
        /// <param name="spindle">The diameter of the central hole in the gear</param>
        /// <param name="inlay">The diameter of the bearing inlay</param>
        /// <param name="inlayDepth">The depth of the bearing inlay</param>

        public Cutouts(GearParameters gear, double thickness, double spindle, double inlay, double inlayDepth)
        {
            if (spindle < 0 || inlay < 0 || inlayDepth < 0)
                throw new ArgumentException("Depths and thicknesses cannot be negative");
            if (inlayDepth >= thickness)
                throw new ArgumentException("Inlay depth too deep for material thickness");
            Gear = gear ?? throw new ArgumentException("No gear specified for cut out");
            Thickness = thickness;
            SpindleDiameter = spindle;
            InlayDiameter = inlay;
            InlayDepth = inlayDepth;
        }

        /// <summary>
        /// Calculate the points that form the spindle circle
        /// </summary>
        /// <returns>Sequence of points that make up the hole
        /// in the middle of the gear</returns>

        public IEnumerable<PointF> CalculateSpindle()
            => Involutes.LinearReduction(Involutes.CirclePoints
                (-Math.PI, Math.PI, Math.PI / 2880, SpindleDiameter / 2).ToList(),
                (float)Gear.MaxError);


        /// <summary>
        /// Calculate the points that form the inlaid circle
        /// </summary>
        /// <returns>Sequence of points that make up the bearing 
        /// inlay in the middle of the gear</returns>

        public IEnumerable<PointF> CalculateInlay()
            => Involutes.LinearReduction(Involutes.CirclePoints
                (-Math.PI, Math.PI, Math.PI / 2880, InlayDiameter / 2).ToList(),
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

        public List<List<PointF>> CalculateCutouts()
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

        public List<List<PointF>> CalculateCutouts(int spokes)
        {
            if (spokes < 3)
                return null;

            // Set some design constants

            double cornerRadius = Gear.Module;
            double spokeThickness = 2.5 * Gear.Module;
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

            double rimDiameter = Gear.CutterAdjustedDedendumCircleDiameter - 1.5 * spokeThickness;
            if (rimDiameter < hubDiameter + 4 * cornerRadius)
                return null;

            double cornerCentreY = spokeThickness / 2 + cornerRadius;
            double rimCornerCentreX = Math.Sqrt(Square(rimDiameter / 2 - cornerRadius)
                - Square(cornerCentreY));
            PointF rimCornerCentre = new PointF((float)rimCornerCentreX, (float)cornerCentreY);
            double angleAtRim = Math.Atan2(cornerCentreY, rimCornerCentreX);
            IEnumerable<PointF> outerCorner = Involutes.CirclePoints
                (-Math.PI / 2, angleAtRim, Math.PI / 2880, cornerRadius, rimCornerCentre);

            // Calculate the corner at the inner end of a spoke.

            double hubCornerCentreX = Math.Sqrt(Square(hubDiameter / 2 + cornerRadius)
                - Square(cornerCentreY));
            PointF hubCornerCentre = new PointF((float)hubCornerCentreX, (float)cornerCentreY);
            double angleAtHub = Math.Atan2(cornerCentreY, hubCornerCentreX);

            IEnumerable<PointF> innerCorner = Involutes.CirclePoints
                (Math.PI + angleAtHub, 3.0 * Math.PI / 2, Math.PI / 2880, cornerRadius, hubCornerCentre);

            // Calculate the outer rim circle segment

            IEnumerable<PointF> outerRimSegment = Involutes.CirclePoints
                (angleAtRim, 2 * Math.PI / spokes - angleAtRim, Math.PI / 2880, rimDiameter / 2);

            // Calculate the hub circle segment

            IEnumerable<PointF> hubSegment = Involutes.CirclePoints
                (angleAtHub, 2 * Math.PI / spokes - angleAtHub, Math.PI / 2880, hubDiameter / 2);

            // Calculate the far side of the cutout. Reflect the inner spoke
            // across the X axis, and reverse its points. Then rotate it round the gear
            // by the angle between adjacent spokes. This gives us the correct
            // list of points.

            IEnumerable<PointF> nearSide = innerCorner.Concat(outerCorner);
            IEnumerable<PointF> farSide = GearParameters.ReflectY(nearSide)
                .Select(p => GearParameters.RotateAboutOrigin(2 * Math.PI / spokes, p))
                .Reverse();

            // Now create the lists of points for each of the cut outs

            List<List<PointF>> cutouts = new List<List<PointF>>();
            List<PointF> cutout = new List<PointF>();
            cutout.AddRange(nearSide);
            cutout.AddRange(outerRimSegment);
            cutout.AddRange(farSide);
            cutout.AddRange(hubSegment.Reverse());
            cutout = Involutes.LinearReduction(cutout, (float)Gear.MaxError);
            cutouts.Add(cutout);
            for (int i = 1; i < spokes; i++)
                cutouts.Add(new List<PointF>(cutout.Select
                    (p => GearParameters.RotateAboutOrigin(2 * Math.PI * i / spokes, p))));
            return cutouts;
        }

        private static double Square(double x) => x * x;
    }
}

