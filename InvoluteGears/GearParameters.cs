using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
    public class GearParameters
    {
        public GearParameters(int toothCount, double module = 1.0, 
            double pressureAngle = Math.PI/9, double profileShift = 0.0)
        {
            ToothCount = toothCount;
            Module = module;
            PressureAngle = pressureAngle;
            ProfileShift = profileShift;
            InitPointLists();
        }

        /// <summary>
        /// The profile shift coefficient for corrected gears.
        /// Used to keep the contact ratio above 1.0 for gears
        /// with numbers of teeth fewer than 17. This figure
        /// is multiplied by the Module to find how far the
        /// dedendum and addendum radii are increased.
        /// </summary>
        
        public double ProfileShift { get; private set; }
        
        /// <summary>
        /// The resolution of points on the various curves
        /// that are plotted as part of the gear profile
        /// generation. This figure is the number of points
        /// in the full rotation of the gear along a rack.
        /// </summary>

        public const int PointsPerRotation = 3600;

        // Two gears that are engaged with each other each
        // have a pitch circle. The pitch circles of the
        // two gears are tangential to each other and have
        // radii that are in the same ratio as the gear
        // ratio between the two gears.

        public double PitchCircleDiameter => Module * ToothCount;

        // The radius of the gear including the projection
        // of teeth by one module beyond the pitch circle
        
        public double AddendumCircleDiameter 
            => PitchCircleDiameter + 2 * Module*(1 + ProfileShift);

        // The maximum non-interfering radius of the inner
        // circle at the foot of the gap between teeth.
        public double DedendumCircleDiameter 
            => PitchCircleDiameter - 2 * Module * (1 - ProfileShift);

        // The number of teeth around the outside of the
        // gear wheel. These are evenly spaced obviously.
        // Note that if this is positive, it is a normal
        // gear. If zero, it is a rack. If negative, it
        // is an internal gear.
        
        public int ToothCount { get; private set; }

        // The angle relative to a line between the centres
        // of two gears at which the gears touch each other.
        // By convention this is chosen to be 14.5, 20, or 25
        // degrees, though there are no design reasons why
        // these specific angles have to be used.
        
        public double PressureAngle { get; private set; }

        // The extra diameter needed per extra tooth for a
        // given tooth width. In practice this value is often
        // used to determine pitch circle diameter and tooth
        // separation, rather than the other way round.

        public double Module { get; private set; }

        // The base circle diameter for the involute curve

        public double BaseCircleDiameter
            => PitchCircleDiameter * Math.Cos(PressureAngle);

        /// <summary>
        /// The distance between adjacent teeth faces along
        /// the line of action, or the curved distance around
        /// the base circle between the start of the involutes
        /// for two adjacent teeth.
        /// </summary>
        
        public double BaseCirclePitch
            => Pitch * Math.Cos(PressureAngle);

        /// <summary>
        /// The distance between adjacent teeth around the pitch circle
        /// </summary>

        public double Pitch
            => Math.PI * Module;

        /// <summary>
        /// The angle from the pitch point to the point
        /// on the base circle at which its involute touches.
        /// At the pitch point two teeth are in contact at the
        /// pressure angle and at the point where the two pitch 
        /// circles meet. Two gears 'roll' on each other at the
        /// pitch circle, and the ratio between two pitch circle
        /// radii is used to derive the gear ratio. This point is
        /// Pi/(2*ToothCount) radians from the tooth centre. It is
        /// also PressureAngle radians from the point at which
        /// its involute through this contact point touches the
        /// base circle. Finally if the line from the pitch point
        /// were wrapped around the outside of the base circle,
        /// the point at which it ends would be at the angle we
        /// are interested in.
        /// </summary>

        public double ToothBaseOffset
            => Math.Tan(PressureAngle) - PressureAngle;

        /// <summary>
        /// The angle from the point at which a tooth involute touches
        /// the base circle to the point where the involute
        /// edge for the tooth reaches the addendum circle
        /// </summary>

        public double AddendumInvoluteAngle
            => Math.Sqrt(Square((AddendumCircleDiameter) / BaseCircleDiameter) - 1);

        /// <summary>
        /// The angle occupied by one tooth and one gap
        /// </summary>
        
        public double ToothAngle => 2 * Math.PI / ToothCount;

        /// <summary>
        /// The thickness of the tooth in radians
        /// measured at the pitch circle and
        /// allowing for possible profile shifting
        /// </summary>
        
        public double ToothWidthAngleAtPitchCircle
            => 2 * (Math.PI / 2 + 2 * ProfileShift * Math.Tan(PressureAngle)) / ToothCount;

        /// <summary>
        /// The thickness of the tooth gap in radians
        /// measured at the pitch circle and
        /// allowing for possible profile shifting
        /// </summary>

        public double GapWidthAngleAtPitchCircle
            => ToothAngle - ToothWidthAngleAtPitchCircle;

        /// <summary>
        /// A measurement of the distance across the tooth gap at its
        /// narrowest point, where the involute tooth edge meets the
        /// undercut trochoid.
        /// </summary>
        
        public double ToothGapAtUndercut =>
            2 * underCutPoint.Y;

        /// <summary>
        /// Calculate the square of the undercut radius
        /// </summary>

        public double SquaredUndercutRadius
            => Square(underCutPoint.X) + Square(underCutPoint.Y);

        /// <summary>
        /// Check that the two gears are compatible for meshing
        /// </summary>
        /// <param name="meshedGear">The other gear with which
        /// we are trying to mesh</param>
        /// <returns>True if the gears are compatible</returns>

        public bool CanMeshWith(GearParameters meshedGear) 
            => meshedGear != null 
            && PressureAngle == meshedGear.PressureAngle 
            && Module == meshedGear.Module;

        /// <summary>
        /// Given we are meshed with another gear where this and
        /// the other gear might have a profile shift, calculate
        /// the contact length for the two gears
        /// </summary>
        /// <param name="meshedGear">The gear with which we are
        /// meshed</param>
        /// <returns>The length of the path along which the two
        /// gears are in contact before breaking apart</returns>
        
        public double ContactDistanceWithGear(GearParameters meshedGear)
        {
            if (!CanMeshWith(meshedGear))
                throw new ArgumentException("Gears have differing modules or pressure angles");
            double distanceBetweenCentres 
                = PitchCircleDiameter / 2 
                + meshedGear.PitchCircleDiameter / 2 
                + Module * (ProfileShift + meshedGear.ProfileShift);
            double distToPitchPoint = distanceBetweenCentres 
                * BaseCircleDiameter / (BaseCircleDiameter + meshedGear.BaseCircleDiameter);
            double meshedDistToPitchPoint = distanceBetweenCentres - distToPitchPoint;
            double contactLength 
                = Math.Sqrt(Square(distToPitchPoint) - Square(BaseCircleDiameter / 2))
                - Math.Sqrt(SquaredUndercutRadius - Square(BaseCircleDiameter / 2));
            double meshedContactLength 
                = Math.Sqrt(Square(meshedDistToPitchPoint) - Square(meshedGear.BaseCircleDiameter / 2))
                - Math.Sqrt(meshedGear.SquaredUndercutRadius - Square(meshedGear.BaseCircleDiameter / 2));
            return contactLength + meshedContactLength;
        }

        /// <summary>
        /// Contact ratio between two gears, allowing
        /// also for possible profile shift
        /// </summary>
        /// <param name="meshedGear">The other gear we
        /// are meshing with</param>
        /// <returns>The contact ratio. Must be greater
        /// than one for gears to mesh correctly.</returns>
        
        public double ContactRatioWith(GearParameters meshedGear)
            => ContactDistanceWithGear(meshedGear) / BaseCirclePitch;

        /// <summary>
        /// Given the selected tooth number, compute the list of points that
        /// make up the side of the tooth anticlockwise from the gap selected.
        /// On the positive X axis is the centre of gap zero between two teeth.
        /// Above it is the anticlockwise involute.
        /// </summary>
        /// <param name="gapNumber">The numbered gap between teeth. Gap zero lies
        /// along the positive X axis, with gaps numbered anticlockwise
        /// from there.</param>
        /// <returns>The list of points making up the involute from the
        /// base circle up to the edge of the addendum</returns>

        public IEnumerable<PointF> AntiClockwiseInvolute(int gapNumber)
        {
            // The angles between the middles of adjacent
            // teeth in radians is 2*PI / ToothCount

            double gapCentreAngle = (gapNumber % ToothCount) * ToothAngle;
            return InvolutePoints.Select(p => RotateAboutOrigin(gapCentreAngle, p));
        }

        private List<PointF> UndercutPoints;
        private List<PointF> InvolutePoints;

        private IEnumerable<PointF> ComputeInvolutePoints()
        {
            double involuteBaseAngle = GapWidthAngleAtPitchCircle / 2 - ToothBaseOffset;

            int limit = AngleIndexFloor(AddendumInvoluteAngle);
            for (int i = limit; i >= 0; i--)
            {
                double angle = (i % PointsPerRotation) * 2 * Math.PI / PointsPerRotation;
                yield return Involutes.InvolutePlusOffset
                    (BaseCircleDiameter / 2, 0, 0, angle, involuteBaseAngle);
            }
        }

        private IEnumerable<PointF> ComputeUndercutPoints()
        {
            int lowerLimit = AngleIndexFloor(-UndercutAngleAtPitchCircle);
            int upperLimit = AngleIndexFloor(DedendumArcAngle / 2);
            for (int i = lowerLimit; i <= upperLimit; i++)
            {
                double angle = (i % PointsPerRotation) * 2 * Math.PI / PointsPerRotation;
                yield return Involutes.InvolutePlusOffset(PitchCircleDiameter / 2, -Module * (1 - ProfileShift),
                    Module * Math.PI / 4 - Module * Math.Sin(PressureAngle), angle, 0);
            }
        }

        private PointF underCutPoint = new PointF(0, 0);
        
        private void InitPointLists()
        {
            InvolutePoints = new List<PointF>(ComputeInvolutePoints());
            UndercutPoints = new List<PointF>(ComputeUndercutPoints());
            var intersection = Involutes.Intersection(InvolutePoints, UndercutPoints);
            if(intersection != null && intersection.HasValue)
            {
                underCutPoint = intersection.Value;
                int involuteIdx = Involutes.IndexOfLastPointWithGreaterXVal(InvolutePoints, underCutPoint.X);
                int undercutIdx = Involutes.IndexOfLastPointWithGreaterXVal(UndercutPoints, underCutPoint.X);
                InvolutePoints.RemoveRange(involuteIdx + 1, InvolutePoints.Count - involuteIdx - 1);
                UndercutPoints.RemoveRange(0, undercutIdx + 1);
            }
        }

        /// <summary>
        /// Reflect a sequence of points to the opposite side of the X axis
        /// </summary>
        /// <param name="points">The points to be reflected</param>
        /// <returns>The reflected points</returns>
        
        private IEnumerable<PointF> ReflectY(IEnumerable<PointF> points)
            => points.Select(p => new PointF(p.X, -p.Y));

        /// <summary>
        /// Rotate a point about the origin in the anticlockwise
        /// direction by the angle phi
        /// </summary>
        /// <param name="phi">The angle to rotate by in radians</param>
        /// <param name="pt">The point from which a rotated
        /// point will be generated</param>
        /// <returns>The rotated point</returns>
        
        private PointF RotateAboutOrigin(double phi, PointF pt)
        {
            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);

            return new PointF((float)(pt.X * cosPhi - pt.Y * sinPhi), 
                (float)(pt.X*sinPhi + pt.Y*cosPhi));
        }

        /// <summary>
        /// Given the selected tooth number, compute the list of points that
        /// make up the side of the tooth clockwise from the gap selected.
        /// On the positive X axis is the centre of gap zero between two teeth.
        /// Below it is the clockwise involute.
        /// </summary>
        /// <param name="gap">The numbered gap between teeth. Gap zero lies
        /// along the positive X axis, with gaps numbered anticlockwise
        /// from there.</param>
        /// <returns>The list of points making up the involute from the
        /// base circle up to the edge of the addendum</returns>

        public IEnumerable<PointF> ClockwiseInvolute(int gap)
        {
            // The angles between the middles of adjacent
            // teeth in radians is 2*PI / ToothCount

            double gapCentreAngle = (gap % ToothCount) * ToothAngle;
            return ReflectY(InvolutePoints).Select(p => RotateAboutOrigin(gapCentreAngle, p));
        }

        /// <summary>
        /// Given the gap number around the gear, calculate the points on the
        /// curve for the undercut from where it is tangential to the dedendum
        /// circle round to where it crosses the pitch circle. This is sufficient
        /// for all gears containing five or more teeth. At four teeth, the
        /// undercut exceeds the pitch circle.This function calculates
        /// the under cut anticlockwise from the gap selected.
        /// </summary>
        /// <param name="gap">The numbered gap between teeth. Gap zero lies
        /// along the positive X axis, with gaps numbered anticlockwise
        /// from there.</param>
        /// <returns>The list of points making up the trochoid from the
        /// dedendum circle up to the pitch circle</returns>

        public IEnumerable<PointF> AnticlockwiseUndercut(int gap)
        {
            if (UndercutPoints == null)
                UndercutPoints = new List<PointF>(ComputeUndercutPoints());

            double gapCentreAngle = (gap % ToothCount) * ToothAngle;
            return UndercutPoints.Select(p => RotateAboutOrigin(gapCentreAngle, p));
        }

        /// <summary>
        /// Given the gap number around the gear, calculate the points on the
        /// curve for the undercut from where it is tangential to the dedendum
        /// circle round to where it crosses the pitch circle. This is sufficient
        /// for all gears containing five or more teeth. At four teeth, the
        /// undercut exceeds the pitch circle. This function calculates
        /// the under cut clockwise from the gap selected.
        /// </summary>
        /// <param name="gap">The numbered gap between teeth. Gap zero lies
        /// along the positive X axis, with gaps numbered anticlockwise
        /// from there.</param>
        /// <returns>The list of points making up the trochoid from the
        /// dedendum circle up to the pitch circle</returns>

        public IEnumerable<PointF> ClockwiseUndercut(int gap)
        {
            if (UndercutPoints == null)
                UndercutPoints = new List<PointF>(ComputeUndercutPoints());

            double gapCentreAngle = (gap % ToothCount) * ToothAngle;
            return ReflectY(UndercutPoints).Select(p => RotateAboutOrigin(gapCentreAngle, p));
        }

        /// <summary>
        /// The angle between the two points on the dedendum circle at which
        /// the two corners of the mating tooth's addendum touch the
        /// dedendum circle. Used for determining where the undercut curve
        /// starts on the dedendum circle.
        /// </summary>

        public double DedendumArcAngle 
            => (Pitch - 4 * Module * Math.Tan(PressureAngle)) / PitchCircleDiameter;

        /// <summary>
        /// The angle from tooth dead centre to edge of tooth crossing pitch
        /// circle when undercutting on trailing edge of tooth on rotation.
        /// Also accommodates profile shifting of the tooth.
        /// </summary>
        
        public double UndercutAngleAtPitchCircle
        {
            get
            {
                double shiftedModule = Module * (1 - ProfileShift);
                double k = Math.Sqrt(shiftedModule * PitchCircleDiameter - Square(shiftedModule));
                double j = k - Module * (Math.PI / 4 - Math.Tan(PressureAngle));
                return 2 * j / PitchCircleDiameter;
            }
        }

        private double Square(double v) => v * v;

        private int AngleIndexFloor(double angle) 
            => (int)(angle * PointsPerRotation / (2 * Math.PI));
    }
}
