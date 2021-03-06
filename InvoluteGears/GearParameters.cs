﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
    public class GearParameters : IGearProfile
    {
        public GearParameters(int toothCount, double module = 1.0,
            double pressureAngle = Math.PI / 9, double profileShift = 0.0,
            double maxErr = 0.0, double backlash = 0.0, double cutterDiam = 0.0)
        {
            ToothCount = toothCount;
            Module = module;
            PressureAngle = pressureAngle;
            ProfileShift = profileShift;
            MaxError = maxErr;
            Backlash = backlash;
            CutDiameter = cutterDiam;
            SetInformation();
            InitPointLists();
        }

        private void SetInformation()
        {
            Information = $"Involute: {ToothCount} teeth, module = {Module}mm, pressure angle = {PressureAngle * 180 / Math.PI:N1}\u00b0\r\n";
            Information += $"profile shift = {ProfileShift * 100:N1}%, precision = {MaxError}mm\r\n";
            Information += $"backlash = {Backlash * Module}mm, cutter diameter = {CutDiameter}mm\r\n";
        }

        public string ShortName
            => $"It{ToothCount}m{Module:N2}a{PressureAngle * 180 / Math.PI:N1}s{ProfileShift:N3}"
                + $"e{MaxError:N2}b{Backlash * Module:N2}c{CutDiameter:N2}.svg";

        /// <summary>
        /// Used for warning or information messages when methods invoked
        /// </summary>

        public string Information { get; private set; }

        /// <summary>
        /// The diameter of the bit used to cut out the gear. In mm.
        /// </summary>

        public double CutDiameter
        {
            get;
            private set;
        }

        /// <summary>
        /// The tolerance for the curved faces of the teeth. This is used to set the
        /// maximum deviation of the points on the curved faces from true value, and
        /// is used to reduce the number of points we plot on each curve. Default
        /// of zero does no reduction. Should be set to a value that matches the
        /// precision of the cutting machine for the gears. Measured in mm.
        /// </summary>

        public double MaxError { get; private set; }

        /// <summary>
        /// The amount of backlash to build into the gear. If left at zero,
        /// theoretically perfect gear surfaces are created, for which the leading
        /// and trailing faces rub against each other with zero gap. By setting
        /// a value for this, we build in a small angular separation between
        /// the trailing face of one tooth and the leading edge of the tooth
        /// behind. We also shorten the tooth addendum radius slightly, while
        /// still computing the undercut for the tooth at full addendum
        /// height. This fractionally reduces the contact ratio, but only
        /// for the larger tooth counts where the full length of the 
        /// involute surface of the tooth might be used. In these cases
        /// the contact ratio is high anyway. In fact, for low tooth
        /// counts, shortening the addendum height actually reduces
        /// undercut if it were computed for the shortened tooth. Here
        /// though we still compute the undercut as if a full length tooth
        /// then shorten the tooth slightly to remove rubbing at the
        /// dedendum surface.
        /// Backlash is measured in fractions of the Module. This
        /// equates to the same fraction of the module for shortening
        /// of the addendum height. However, we don't multiply by PI
        /// circumferentially, we take the fraction of the module
        /// as measured at the pitch circle. Hence at the pitch
        /// circle for a backlash of 0.2 and a module of 5mm,
        /// both radially and circumferentially we would get a
        /// backlash of 1mm.
        /// </summary>

        public double Backlash { get; private set; }

        /// <summary>
        /// The profile shift coefficient for corrected gears.
        /// Used to keep the contact ratio above 1.0 for gears
        /// with numbers of teeth fewer than 17. This figure
        /// is multiplied by the Module to find how far the
        /// dedendum and addendum radii are increased. Hence
        /// it has units of fractions of the module.
        /// </summary>

        public double ProfileShift { get; private set; }

        /// <summary>
        /// Two gears that are engaged with each other each
        /// have a pitch circle. The pitch circles of the
        /// two gears are tangential to each other and have
        /// radii that are in the same ratio as the gear
        /// ratio between the two gears.
        /// </summary>

        public double PitchCircleDiameter => Module * ToothCount;

        /// <summary>
        /// The radius of the gear including the projection
        /// of teeth by one module beyond the pitch circle.
        /// Adjustments are also made if the gear has been 
        /// profile shifted, and if the tooth has been 
        /// shortened for backlash (to prevent the addendum
        /// of one tooth grinding on the dedendum of its
        /// partner)
        /// </summary>

        public double AddendumCircleDiameter
            => PitchCircleDiameter
            + 2 * Module * (1 + ProfileShift - Backlash);

        /// <summary>
        /// The maximum non-interfering radius of the inner
        /// circle at the foot of the gap between teeth.
        /// Allowance has been made for profile shifting
        /// of the teeth.
        /// </summary>

        public double DedendumCircleDiameter
            => PitchCircleDiameter - 2 * Module
            * (1 - ProfileShift);

        private double? cutterAdjustedDedendumDircleDiameter;

        /// <summary>
        /// When we are using an end mill to cut out the teeth,
        /// there will be a minimum diameter for concave curves
        /// as they cannot be less than the diameter of the
        /// cutter. As a consequence, the shape of the dedendum
        /// can be modified to allow for the cutter diameter,
        /// meaning that the dedendum may dig deeper into the
        /// gear, but with shallower turns.
        /// </summary>

        public double InnerDiameter
        {
            get
            {
                if (cutterAdjustedDedendumDircleDiameter.HasValue)
                    return cutterAdjustedDedendumDircleDiameter.Value;
                else
                    return DedendumCircleDiameter;
            }
        }

        /// <summary>
        /// The number of teeth around the outside of the
        /// gear wheel. These are evenly spaced obviously.
        /// Note that if this is positive, it is a normal
        /// gear. If zero, it is a rack. If negative, it
        /// is an internal gear.
        /// </summary>

        public int ToothCount { get; private set; }

        /// <summary>
        /// The angle relative to a line between the centres
        /// of two gears at which the gears touch each other.
        /// By convention this is chosen to be 14.5, 20, or 25
        /// degrees, though there are no design reasons why
        /// these specific angles have to be used. Units are
        /// in radians, not degrees. 14.5 degrees = 0.25307
        /// radians. 20 degrees = 0.34907, and 25 degrees
        /// = 0.43633 radians.
        /// </summary>

        public double PressureAngle { get; private set; }

        /// <summary>
        /// The extra diameter needed per extra tooth for a
        /// given tooth width. In practice this value is often
        /// used to determine pitch circle diameter and tooth
        /// separation, rather than the other way round. Units
        /// in mm.
        /// </summary>

        public double Module { get; private set; }

        // -- DERIVED PROPERTIES -- //

        /// <summary>
        /// The base circle diameter for the involute curve
        /// </summary>

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

        private double ToothBaseOffset
            => Math.Tan(PressureAngle) - PressureAngle;

        /// <summary>
        /// The angle from the pitch point to the point on the
        /// addendum circle at which the involute touches.
        /// </summary>

        private double ToothTipOffset
            => AddendumInvoluteAngle
            - Math.Acos(BaseCircleDiameter / AddendumCircleDiameter)
            - ToothBaseOffset;

        /// <summary>
        /// The angle from the point at which a tooth involute touches
        /// the base circle to the point where the involute
        /// edge for the tooth reaches the addendum circle
        /// </summary>

        private double AddendumInvoluteAngle
            => Math.Sqrt(Involutes.Square(AddendumCircleDiameter / BaseCircleDiameter) - 1);

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
        /// undercut trochoid. Does not include backlash widening.
        /// </summary>

        public double ToothGapAtUndercut =>
            2 * underCutPoint.Y;

        /// <summary>
        /// Calculate the square of the undercut radius
        /// </summary>

        private double SquaredUndercutRadius
            => Involutes.SumOfSquares(underCutPoint.X, underCutPoint.Y);

        /// <summary>
        /// Obtain the radius of the point at which the gear's
        /// involute working surface changes to the undercut
        /// trochoid. This is the point at which the gear teeth
        /// interaction no longer follows the involute curve.
        /// </summary>

        public double UndercutRadius => Math.Sqrt(SquaredUndercutRadius);

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
            {
                Information = "Gears have differing modules or pressure angles";
                return 0;
            }
            double distanceBetweenCentres
                = PitchCircleDiameter / 2
                + meshedGear.PitchCircleDiameter / 2
                + Module * (ProfileShift + meshedGear.ProfileShift);
            double distToPitchPoint = distanceBetweenCentres
                * BaseCircleDiameter / (BaseCircleDiameter + meshedGear.BaseCircleDiameter);
            double meshedDistToPitchPoint = distanceBetweenCentres - distToPitchPoint;
            double contactLength
                = Math.Sqrt(Involutes.DiffOfSquares(distToPitchPoint, BaseCircleDiameter / 2))
                - Math.Sqrt(SquaredUndercutRadius - Involutes.Square(BaseCircleDiameter / 2));
            double meshedContactLength
                = Math.Sqrt(Involutes.DiffOfSquares(meshedDistToPitchPoint, meshedGear.BaseCircleDiameter / 2))
                - Math.Sqrt(meshedGear.SquaredUndercutRadius - Involutes.Square(meshedGear.BaseCircleDiameter / 2));
            return contactLength + meshedContactLength;
        }

        /// <summary>
        /// Contact ratio between two gears, allowing
        /// also for possible profile shift. Contact
        /// ratio is defined as the average number of
        /// teeth in contact between two gears as they
        /// rotate. If this is less than 1.0, there are
        /// times at which no teeth are meshing on the
        /// involute surfaces and the gears will
        /// vibrate or lock up.
        /// </summary>
        /// <param name="meshedGear">The other gear we
        /// are meshing with</param>
        /// <returns>The contact ratio. Must be greater
        /// than one for gears to mesh correctly.</returns>

        public double ContactRatioWith(GearParameters meshedGear)
            => ContactDistanceWithGear(meshedGear) / BaseCirclePitch;

        private List<PointF> UndercutPoints;
        private List<PointF> InvolutePoints;
        private List<PointF> DedendumPoints;
        private List<PointF> AddendumPoints;

        private IEnumerable<PointF> ComputeInvolutePoints()
        {
            double involuteBaseAngle = GapWidthAngleAtPitchCircle / 2 - ToothBaseOffset;

            int limit = AngleIndexFloor(AddendumInvoluteAngle);
            for (int i = limit; i >= 0; i--)
            {
                double angle = (i % Involutes.PointsPerRotation) * Involutes.AngleStep;
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
                double angle = (i % Involutes.PointsPerRotation) * Involutes.AngleStep;
                yield return Involutes.InvolutePlusOffset(PitchCircleDiameter / 2,
                    -Module * (1 - ProfileShift),
                    Module * (Math.PI / 4 - Math.Tan(PressureAngle)), angle, 0);
            }
        }

        /// <summary>
        /// Once the undercut points and the dedendum points have been calculated,
        /// but before the number of points are reduced based on an error tolerance,
        /// any concave parts of the tooth profile may be too sharp cornered for
        /// a given cutter radius. This method redraws the undercut and dedendum
        /// profile so that it can accommodate the diameter of cutter we are
        /// intending to cut the gear with.
        /// </summary>
        /// <param name="cutterRadius">Radius of the end mill bit in mm</param>
        /// <returns>True if the points had to be adjusted, false if the
        /// curves were all sufficiently shallow that adjustment did
        /// not take place</returns>

        private bool AdjustPointsForCircularCutter()
        {
            // First find the point at which and end-mill of the specified cutter
            // radius can no longer cut inside the concave profile of the undercut

            int i = 0;
            bool cornerFound = false;
            PointF cutterCentre = PointF.Empty;
            while (!cornerFound && i < UndercutPoints.Count - 2)
            {
                PointF[] centres = Involutes.CircleCentres(UndercutPoints[i], UndercutPoints[i + 1], CutDiameter / 2);
                if (centres[0].Y < centres[1].Y)
                    cutterCentre = centres[0];
                else
                    cutterCentre = centres[1];
                cornerFound = Involutes.PointInCircle(UndercutPoints[i + 2], cutterCentre, CutDiameter / 2);
                i++; // i indexes the last point from the undercut point list that we can cut to
            }

            // If no adjustment took place, quit here

            if (!cornerFound)
                return false;

            // Copy across the curve that we are able to follow before
            // deviating from it according to cutter radius

            UndercutPoints = new List<PointF>(UndercutPoints.Take(i + 1));

            // Calculate the angle for the last point in the undercut curve

            double startAngle = Math.Atan2(UndercutPoints.Last().Y - cutterCentre.Y,
                UndercutPoints.Last().X - cutterCentre.X);

            // Find the point on the cutter circle that intersects a line from
            // its centre to the centre of the gear profile (origin 0,0)

            double cutterCentreRadius = Math.Sqrt(Involutes.SumOfSquares(cutterCentre.X, cutterCentre.Y));

            // Find the end angle for the cutter radius curve. This is the same as
            // the angle at the origin to the cutter centre, reflected by 180 degrees.

            double endAngle = Math.Atan2(cutterCentre.Y, cutterCentre.X) + Math.PI;

            // Add points around the cutter diameter from the last point
            // of adjustedUndercut, to the point at which it crosses yDedendum (y value).

            UndercutPoints.AddRange(Involutes.CirclePoints
                (startAngle, endAngle, Math.PI / 180, CutDiameter / 2, cutterCentre));

            // Then add new dedendum circle points round to the y=0 axis, based on
            // the tangent to the cutter circle at yDedendum.

            DedendumPoints = new List<PointF>(Involutes.CirclePoints
                (-(BacklashAngle + endAngle - Math.PI), endAngle - Math.PI,
                Involutes.AngleStep, cutterCentreRadius - CutDiameter / 2));

            // Record the new dedendum diameter since the cutter has reduced it

            cutterAdjustedDedendumDircleDiameter = 2 * cutterCentreRadius - CutDiameter;

            // Undercut points and dedendum points were rewritten. Return
            // true to flag this fact.

            return true;
        }

        private IEnumerable<PointF> ComputeDedendumCirclePoints()
            => Involutes.CirclePoints
                (-(BacklashAngle + DedendumArcAngle / 2), DedendumArcAngle / 2,
                Involutes.AngleStep, DedendumCircleDiameter / 2);

        private IEnumerable<PointF> ComputeAddendumCirclePoints()
            => Involutes.CirclePoints
                    (GapWidthAngleAtPitchCircle / 2 + ToothTipOffset,
                    ToothAngle - GapWidthAngleAtPitchCircle / 2 - ToothTipOffset - BacklashAngle,
                    Involutes.AngleStep, AddendumCircleDiameter / 2);

        private PointF underCutPoint = PointF.Empty;

        private void InitPointLists()
        {
            InvolutePoints = new List<PointF>(ComputeInvolutePoints());
            UndercutPoints = new List<PointF>(ComputeUndercutPoints());
            DedendumPoints = new List<PointF>(ComputeDedendumCirclePoints());
            PointF? intersection = null;
            if (intersection == null || !intersection.HasValue)
                intersection = Involutes.ClosestPoint(InvolutePoints, UndercutPoints);
            underCutPoint = intersection.Value;
            int involuteIdx = Involutes.IndexOfLastPointWithGreaterXVal(InvolutePoints, underCutPoint.X);
            int undercutIdx = Involutes.IndexOfLastPointWithGreaterXVal(UndercutPoints, underCutPoint.X);
            InvolutePoints.RemoveRange(involuteIdx + 1, InvolutePoints.Count - involuteIdx - 1);
            UndercutPoints.RemoveRange(0, undercutIdx + 1);
            if (CutDiameter > 0 && AdjustPointsForCircularCutter())
                Information += "Undercut and dedendum adjusted for cutter diameter\r\n";
            if (ToothGapAtUndercut < CutDiameter)
                Information += $"Cutter dia. {CutDiameter} too wide for tooth gap of {ToothGapAtUndercut}\r\n";

            // TODO: Ideally should use AdjustPointsForCircularCutter return
            // value to prevent reduction in tooth addendum height used for
            // backlash management. At present this is not done. The tooth
            // height is still reduced by the backlash amount, even though
            // the undercut has increased the dedendum depth to accommodate
            // the diameter of cutter.

            AddendumPoints = new List<PointF>(ComputeAddendumCirclePoints());
            InvolutePoints = Involutes.LinearReduction(InvolutePoints, (float)MaxError);
            UndercutPoints = Involutes.LinearReduction(UndercutPoints, (float)MaxError);
            DedendumPoints = Involutes.LinearReduction(DedendumPoints, (float)MaxError);
            AddendumPoints = Involutes.LinearReduction(AddendumPoints, (float)MaxError);
        }

        /// <summary>
        /// Generate the complete outline of the
        /// whole gear in a single list of points.
        /// </summary>
        /// <returns>The points that make up the outline of
        /// the gear. The last point should be joined back
        /// onto the first point to close the outline.</returns>

        public IEnumerable<PointF> GenerateCompleteGearPath() => Enumerable
                .Range(0, ToothCount)
                .Select(i => GeneratePointsForOnePitch(i))
                .SelectMany(ep => ep)
                .SelectMany(p => p);

        public IEnumerable<IEnumerable<PointF>> GeneratePointsForOnePitch(int i)
        {
            yield return ClockwiseInvolute(i);
            yield return ClockwiseUndercut(i);
            yield return Dedendum(i);
            yield return AnticlockwiseUndercut(i);
            yield return AnticlockwiseInvolute(i);
            yield return Addendum(i);
        }

        /// <summary>
        /// Reflect a sequence of points to the opposite side of the X axis
        /// </summary>
        /// <param name="points">The points to be reflected</param>
        /// <returns>The reflected points</returns>

        public static IEnumerable<PointF> ReflectY(IEnumerable<PointF> points)
            => points.Select(p => Involutes.CreatePt(p.X, -p.Y));

        /// <summary>
        /// The angle around the gear subtended by the Backlash
        /// property value. Backlash is measured as a fraction
        /// of the Module, but we don't want to have units
        /// scaled up by PI around the gear but not radially.
        /// Hence we define backlash as the fraction of the
        /// module length around the pitch circle.
        /// </summary>

        private double BacklashAngle
            => 2 * Backlash / ToothCount;

        /// <summary>
        /// Given a list of points, rotate them about the origin
        /// by a whole number of tooth angles
        /// </summary>
        /// <param name="points">The points to rotate about the origin</param>
        /// <param name="gap">The gap number for the tooth, in the
        /// range 0 to ToothCount - 1</param>
        /// <returns>The rotated set of points</returns>

        private IEnumerable<PointF> RotateByTeeth(IEnumerable<PointF> points, int gap)
            => Involutes.RotateAboutOrigin((gap % ToothCount) * ToothAngle, points);

        /// <summary>
        /// Given the selected tooth number, compute the list of points that
        /// make up the side of the tooth anticlockwise from the gap selected.
        /// On the positive X axis is the centre of gap zero between two teeth.
        /// Above it is the anticlockwise involute.
        /// </summary>
        /// <param name="gap">The numbered gap between teeth. Gap zero lies
        /// along the positive X axis, with gaps numbered anticlockwise
        /// from there.</param>
        /// <returns>The list of points making up the involute from the
        /// base circle up to the edge of the addendum</returns>

        public IEnumerable<PointF> AnticlockwiseInvolute(int gap)
            => RotateByTeeth(InvolutePoints, gap).Reverse();

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
            => ReflectAndAddBacklash(InvolutePoints, gap);

        public IEnumerable<PointF> Dedendum(int gap)
            => RotateByTeeth(DedendumPoints, gap);

        public IEnumerable<PointF> Addendum(int gap)
            => RotateByTeeth(AddendumPoints, gap);

        private IEnumerable<PointF> ReflectAndAddBacklash(IEnumerable<PointF> points, int gap)
        {
            // The angles between the middles of adjacent
            // teeth in radians is 2*PI / ToothCount

            double gapCentreAngle = (gap % ToothCount) * ToothAngle - BacklashAngle;
            return ReflectY(points).Select(p => Involutes.RotateAboutOrigin(gapCentreAngle, p));
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
            double gapCentreAngle = (gap % ToothCount) * ToothAngle;
            return UndercutPoints
                .Select(p => Involutes.RotateAboutOrigin(gapCentreAngle, p))
                .Reverse();
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
            => ReflectAndAddBacklash(UndercutPoints, gap);

        /// <summary>
        /// The angle between the two points on the dedendum circle at which
        /// the two corners of the mating tooth's addendum touch the
        /// dedendum circle. Used for determining where the undercut curve
        /// starts on the dedendum circle.
        /// </summary>

        private double DedendumArcAngle
            => (Pitch - 4 * Module * Math.Tan(PressureAngle)) / PitchCircleDiameter;

        /// <summary>
        /// The angle from tooth dead centre to edge of tooth crossing pitch
        /// circle when undercutting on trailing edge of tooth on rotation.
        /// Also accommodates profile shifting of the tooth.
        /// </summary>

        private double UndercutAngleAtPitchCircle
        {
            get
            {
                double shiftedModule = Module * (1 - ProfileShift);
                double k = Math.Sqrt(shiftedModule * PitchCircleDiameter - Involutes.Square(shiftedModule));
                double j = k - Module * (Math.PI / 4 - Math.Tan(PressureAngle));
                return 2 * j / PitchCircleDiameter;
            }
        }

        private static int AngleIndexFloor(double angle)
            => (int)(angle / Involutes.AngleStep);

        /// <summary>
        /// Given an overall gear ratio (numerator/denominator) and the minimum
        /// and maximum number of teeth on each gear to search between, find two
        /// gear and pinion pairs that would produce the desired ratio back onto
        /// the original spindle. Examples of the use would be minute hand to
        /// hour hand reduction where the minute and hour hand are coaxial.
        /// </summary>
        /// <param name="numerator">Numerator part of the desired gear ratio</param>
        /// <param name="denominator">Denominator part of the desired gear ratio</param>
        /// <param name="minTeeth">Minimum tooth count tolerated on the pinions</param>
        /// <param name="maxTeeth">Maximum tooth count tolerated on the gears</param>
        /// <returns>A string with all workable gear and pinion tooth counts,
        /// together with the overall separation between spindles measured in
        /// multiples of the module</returns>

        public static string MatchedPairs(int numerator, int denominator, int minTeeth, int maxTeeth)
        {
            string result = string.Empty;
            for (int b = minTeeth; b <= maxTeeth; b++)
                for (int c = minTeeth; c <= maxTeeth; c++)
                    for (int d = minTeeth; d <= maxTeeth; d++)
                        if (denominator * ((c + d - b) * c) == numerator * b * d)
                            result += $"{c + d - b}/{b} * {c}/{d}, {c + d}\r\n";
            return result;
        }
    }
}
