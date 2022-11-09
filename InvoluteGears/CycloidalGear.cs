﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDimensionLib;

namespace InvoluteGears
{
    public class CycloidalGear : IGearProfile
    {
        public CycloidalGear(int toothCount, int oppositeToothCount, double toothBlunting,
            double oppositeToothBlunting, double module,
            double maxErr, double backlash, double cutterDiam)
        {
            ToothCount = toothCount;
            ToothBlunting = toothBlunting;
            Module = module;
            MaxError = maxErr;
            Backlash = backlash;
            OpposingToothCount = oppositeToothCount;
            OpposingToothBlunting = oppositeToothBlunting;
            CutDiameter = cutterDiam;
            Errors = String.Empty;
            InitPointLists();
            Information = SetInformation();
        }

        private string SetInformation()
        {
            StringBuilder info = new();
            info.Append($"Cycloid: {ToothCount}/{OpposingToothCount} teeth, module = {Module}mm,\r\n");
            info.Append($"blunting = {ToothBlunting * 100:N0}%/{OpposingToothBlunting * 100:N0}%\r\n");
            info.Append($"backlash = {Backlash * Module}mm, cutter diameter = {CutDiameter}mm\r\n");
            info.Append($"precision = {MaxError}mm, contact ratio = {(pinionAddendumAngle + pinionDedendumAngle) / ToothAngle:N3}\r\n");
            info.Append($"max pressure angles: {180 / Math.PI * maxPinionPressureAngle:N2}\u00b0/{180 / Math.PI * maxWheelPressureAngle:N2}\u00b0\r\n");
            if (!string.IsNullOrWhiteSpace(Errors))
                info.Append(Errors);
            return info.ToString();
        }

        public string ShortName
            => $"Ct{ToothCount}_{OpposingToothCount}m{Module:N2}a{ToothBlunting * 100}_{OpposingToothBlunting}"
                + $"e{MaxError:N2}b{Backlash * Module:N2}c{CutDiameter:N2}";

        public string Information { get; private set; }

        /// <summary>
        /// Error messages generated by the gear generators. If an
        /// empty string, the gear generation has succeeded and the
        /// other fields will contain data. If non-empty the other
        /// fields may be empty or null.
        /// </summary>

        public string Errors { get; private set; }

        public int ToothCount { get; private set; }

        /// <summary>
        /// The fraction of the tooth's angle that is unused
        /// for the epicycloid in contact with the opposite
        /// tooth hypocycloid. This unused fraction appears
        /// as a piece of addendum circle at the tooth tip. It
        /// can range from 1.0 where the tooth is effectively
        /// sawn off at the pitch circle, to 0.0 where the
        /// epicycloid runs right up to a point at half the
        /// tooth width. The latter gives the maximum contact
        /// ratio, but steeper maximum pressure angles. The
        /// former gives very low pressure angles but reduces
        /// the contact ratio possibly below the minimum of 1.0.
        /// </summary>

        public double ToothBlunting { get; private set; }

        public double Module { get; private set; }

        public double MaxError { get; private set; }

        public double InnerDiameter => 2 * pinionDedendumRadius;

        public double CutDiameter { get; private set; }

        /// <summary>
        /// Backlash introduced to prevent rubbing of leading and 
        /// trailing tooth surfaces in poorly machined gears.
        /// Backlash is measured in fractions of the module around
        /// the pitch circle. Hence a backlash of 0.2 with a module
        /// of 5mm would lead to a distance of 1mm around the
        /// pitch circle. This backlash is introduced into one gear,
        /// meaning that two gears interleaving will have the sum
        /// of their backlashes as the total backlash between the
        /// gears.
        /// </summary>

        public double Backlash { get; private set; }

        public int OpposingToothCount { get; private set; }


        /// <summary>
        /// The fraction of the opposite tooth's angle that
        /// is unused for the epicycloid in contact with the 
        /// tooth hypocycloid. This unused fraction appears
        /// as a piece of addendum circle at the tooth tip. It
        /// can range from 1.0 where the tooth is effectively
        /// sawn off at the pitch circle, to 0.0 where the
        /// epicycloid runs right up to a point at half the
        /// tooth width. The latter gives the maximum contact
        /// ratio, but steeper maximum pressure angles. The
        /// former gives very low pressure angles but reduces
        /// the contact ratio possibly below the minimum of 1.0.
        /// </summary>

        public double OpposingToothBlunting { get; private set; }

        public double PitchRadius => ToothCount * Module / 2;


        // The total angle for which the pinion and wheel teeth are in contact
        // at the pinion is pinionUpperAngle + pinionLowerAngle. The point between
        // these two angles lies on the line of centres where the two pitch
        // circles touch each other. At the wheel, the angle is different because
        // the module of the wheel is different. This is the sum of wheelUpperAngle
        // and wheelLowerAngle.

        private double pinionDedendumAngle;
        private double pinionAddendumAngle;
        private double wheelAddendumAngle;
        private double wheelDedendumAngle;

        // The radii of the pinion and wheel out to the ends of the active parts
        // of their teeth. Ideally the teeth should curve gracefully beyond that
        // radius, but these radii are the points of furthest contact between teeth.

        private double pinionAddendumRadius;
        private double wheelAddendumRadius;
        private double pinionDedendumRadius;
        //private double wheelDedendumRadius;
        private double maxPinionPressureAngle;
        private double maxWheelPressureAngle;
        private IList<Coordinate>? oneToothProfile;

        public double AddendumDiameter => pinionAddendumRadius * 2;

        private static double MaxAngleFunc(double rp, double rk, double phi, double t)
        {
            // First calculate the position on the epicycloid after the locus
            // wheel has rolled round the pinion by phi radians

            double c = rp + rk;
            double d = phi * c / rk;
            double sinpt = Math.Sin(phi + t);
            double sindt = Math.Sin(d + t);
            return rk * sindt - c * sinpt;
        }

        private static double CalcMaxAddendumAngle(double radius, double otherLocusRadius, int toothCount, double blunting)
        {
            double maxTipAngle = Math.Min(Math.PI * otherLocusRadius / radius, Math.PI);
            double func(double phi)
                => MaxAngleFunc(radius, otherLocusRadius, phi,
                    -0.5 * (1 - blunting) * Math.PI / toothCount);
            return Geometry.RootBinarySearch(func, 0, maxTipAngle, Math.PI / 2048); // Approx 0.1 degree resolution
        }

        private double BacklashAngle =>
            Backlash * Module / PitchRadius;

        private void CalcMaximumCriteria(double locusRadius,
            double wheelLocusRadius, double pinionBlunting, double wheelBlunting)
        {
            double wheelRadius = OpposingToothCount * Module / 2;
            double centres = PitchRadius + wheelRadius;
            pinionAddendumAngle = CalcMaxAddendumAngle
                (PitchRadius, wheelLocusRadius, ToothCount, pinionBlunting);
            wheelAddendumAngle = CalcMaxAddendumAngle
                (wheelRadius, locusRadius, OpposingToothCount, wheelBlunting);
            pinionAddendumRadius = Geometry.Epicycloid
                (PitchRadius, wheelLocusRadius, pinionAddendumAngle).Magnitude;
            wheelAddendumRadius = Geometry.Epicycloid
                (wheelRadius, locusRadius, wheelAddendumAngle).Magnitude;
            pinionDedendumRadius = centres - wheelAddendumRadius;
            //wheelDedendumRadius = centres - pinionAddendumRadius;
            pinionDedendumAngle = wheelRadius / PitchRadius * wheelAddendumAngle;
            wheelDedendumAngle = PitchRadius / wheelRadius * pinionAddendumAngle;
            maxPinionPressureAngle = 0.5 * pinionAddendumAngle * PitchRadius / wheelLocusRadius;
            maxWheelPressureAngle = 0.5 * wheelAddendumAngle * wheelRadius / locusRadius;
        }

        /// <summary>
        /// The angle in radians taken up by one tooth and one dedendum
        /// </summary>

        public double ToothAngle => 2 * Math.PI / ToothCount;

        private double PinionContactRatio =>
            (pinionAddendumAngle + pinionDedendumAngle) / ToothAngle;

        private double WheelContactRatio =>
            (wheelAddendumAngle + wheelDedendumAngle) / (2 * Math.PI) * OpposingToothCount;

        private void InitPointLists()
        {
            // Assume clock toothing with locii radius half of wheel radius

            CalcMaximumCriteria(ToothCount * Module / 4,
                OpposingToothCount * Module / 4, ToothBlunting, OpposingToothBlunting);
            if (PinionContactRatio < 1)
                Errors += "Gear needs more teeth for contact ratio >= 1.0\r\n";
            if (WheelContactRatio < 1)
                Errors += "Opposite gear needs more teeth for contact ratio >= 1.0\r\n";
            oneToothProfile = OneToothProfile();
            if (oneToothProfile == null)
                Errors += "Unable to form the profile for each tooth of this gear\r\n";
        }

        /// <summary>
        /// Generate the points for the raised part of the tooth,
        /// being the part that sticks out beyond the pitch circle
        /// </summary>
        /// <returns>The set of points for the epicycloidal part
        /// of one side of the tooth</returns>

        private IEnumerable<Coordinate> EpicycloidPoints()
        {
            double angle = 0;
            Coordinate cycloidPt = new(PitchRadius, 0);
            while (cycloidPt.Phase < ToothAngle / 4
                && cycloidPt.Phase < pinionAddendumAngle
                && cycloidPt.Magnitude < pinionAddendumRadius)
            {
                yield return cycloidPt;
                angle += Geometry.AngleStep;
                cycloidPt = Geometry.Epicycloid
                    (PitchRadius, OpposingToothCount * Module / 4, angle);
            }
        }

        /// <summary>
        /// Generate the points for the raised part of the tooth,
        /// being the part that sticks out beyond the pitch circle
        /// </summary>
        /// <returns>The set of points for the epicycloidal part
        /// of one side of the tooth</returns>

        private IEnumerable<Coordinate> HypocycloidPoints()
        {
            double angle = 0;
            Coordinate cycloidPt = new(PitchRadius, 0);
            while (cycloidPt.Phase < ToothAngle / 4
                && cycloidPt.Phase < pinionDedendumAngle
                && cycloidPt.Magnitude > pinionDedendumRadius)
            {
                yield return cycloidPt.Conjugate;
                angle += Geometry.AngleStep;
                cycloidPt = Geometry.Hypocycloid
                    (PitchRadius, PitchRadius / 2, angle);
            }
        }

        private IList<Coordinate> OneToothProfile()
        {
            List<Coordinate> cycloids = new(HypocycloidPoints()
                .Reverse()
                .Concat(EpicycloidPoints()));
            double boundaryAngle = cycloids[0].Phase;

            // Start with the dedendum circle
            // points before the rising cycloids

            List<Coordinate> toothProfile;
            if (CutDiameter <= 0)
                toothProfile = new(Geometry.CirclePoints
                    (-ToothAngle / 2 - boundaryAngle - BacklashAngle, boundaryAngle,
                    Geometry.AngleStep, pinionDedendumRadius));
            else
                toothProfile = CutterAdjustedDedendum();

            double dedendumWidth = (toothProfile[0] - toothProfile[^1]).Magnitude;
            if (dedendumWidth < CutDiameter)
                Errors += $"Dedendum width {dedendumWidth:N2} too narrow for cutter\r\n";

            // Now add the rising face of the dedendum then addendum cycloids

            toothProfile.AddRange(cycloids);

            // Compute and add the addendum if one exists

            boundaryAngle = cycloids.Last().Phase;
            List<Coordinate> addendumCircle;
            if (CutDiameter <= 0)
                addendumCircle = new(Geometry.CirclePoints
                    (boundaryAngle, ToothAngle / 2 - boundaryAngle - BacklashAngle,
                    Geometry.AngleStep, pinionAddendumRadius));
            else
                addendumCircle = RoundedAddendum
                    (cycloids[^2], cycloids[^1], CutDiameter / 4);
            toothProfile.AddRange(addendumCircle);
            toothProfile.AddRange(cycloids
                .Select(p => p.Conjugate.Rotate(ToothAngle / 2 - BacklashAngle))
                .Reverse());
            return Geometry.LinearReduction(toothProfile, MaxError);
        }

        List<Coordinate> RoundedAddendum(Coordinate penult, Coordinate ult, double roundingRadius)
        {
            double slope = (ult - penult).Phase;
            Coordinate centre = ult + new Coordinate(roundingRadius, 0)
                .Rotate(slope + Math.PI / 2);
            if (centre.Phase > ToothAngle / 4 - BacklashAngle / 2)
            {
                // Rounding radius too great for width of addendum tip. We
                // need to draw a single arc from the end of one epicycloid
                // arc to the start of the next

                // Find crossing point of perpendicular drawn from epicycloid
                // endpoint to where it crosses a line drawn from the pinion
                // centre through the centre of the tooth addendum

                double m0 = (ult - centre).Gradient;
                double c0 = ult.Y - m0 * ult.X;
                Coordinate crossingPt = Geometry
                    .LineIntersection(m0, c0,
                    Math.Tan(ToothAngle / 4 - BacklashAngle / 2), 0);

                // Now draw the arc joining the ends of the epicycloid segments

                double startAngle = Math.Atan(m0);
                double endAngle = -startAngle + ToothAngle / 2 - BacklashAngle;
                double radius = (ult - crossingPt).Magnitude;
                return new List<Coordinate>(
                    Geometry.CirclePoints(startAngle, endAngle,
                    Geometry.AngleStep, radius, crossingPt));
            }
            else
            {
                // Rounding radius small enough to draw arc, line, arc for
                // the addendum shape. The arcs are set to be tangential
                // to the epicycloid at the start and end of the addendum.

                Coordinate upperCentre = centre
                    .Rotate(ToothAngle / 2 - BacklashAngle - 2 * centre.Phase);

                List<Coordinate> addendumPoints = new();
                addendumPoints.AddRange(Geometry
                    .CirclePoints(slope - Math.PI / 2,
                        ToothAngle / 4 - BacklashAngle / 2, Geometry.AngleStep,
                        roundingRadius, centre));
                addendumPoints.AddRange(Geometry
                    .CirclePoints(ToothAngle / 4 - BacklashAngle / 2,
                    ToothAngle / 2 - BacklashAngle - slope + Math.PI / 2,
                    Geometry.AngleStep, roundingRadius,
                    upperCentre));
                return addendumPoints;
            }
        }

        List<Coordinate> CutterAdjustedDedendum()
        {
            // Find the centres of the two cutter circles

            Coordinate upperCentre = new(pinionDedendumRadius, -CutDiameter / 2);
            Coordinate lowerCentre =
                new Coordinate(pinionDedendumRadius, CutDiameter / 2)
                    .Rotate(-ToothAngle / 2 - BacklashAngle);

            // Find the starting and ending angles for each curve

            double upperEndAngle = Math.PI / 2;
            double dedendumAngle = Math.PI - ToothAngle / 4 - BacklashAngle / 2;
            double lowerStartAngle = 3 * Math.PI / 2 - ToothAngle / 2 - BacklashAngle;
            var points = new List<Coordinate>();
            points.AddRange(Geometry.CirclePoints(dedendumAngle,
                lowerStartAngle, Geometry.AngleStep,
                CutDiameter / 2, lowerCentre)
                .Reverse());
            points.AddRange(Geometry.CirclePoints(upperEndAngle,
                dedendumAngle, Geometry.AngleStep,
                CutDiameter / 2, upperCentre)
                    .Reverse());
            return points;
        }

        public IEnumerable<Coordinate> ToothProfile(int gap)
            => oneToothProfile
                .Rotated((gap % ToothCount) * ToothAngle);

        /// <summary>
        /// Generate the complete path of
        /// points for the whole gear wheel
        /// </summary>
        /// <returns>The set of points 
        /// describing the gear wheel
        /// </returns>

        public IEnumerable<Coordinate> GenerateCompleteGearPath()
        {
            if (!string.IsNullOrWhiteSpace(Errors))
                return Enumerable.Empty<Coordinate>();
            else
                return Enumerable
                    .Range(0, ToothCount)
                    .Select(i => ToothProfile(i))
                    .SelectMany(p => p);
        }

        public DrawablePath GenerateGearCurve()
        {
            throw new NotImplementedException();
        }
    }
}
