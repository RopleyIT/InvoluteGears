﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            Errors = InitPointLists();
            SetInformation();
        }
        private void SetInformation()
        {
            Information = $"Cycloid: {ToothCount}/{OpposingToothCount} teeth, module = {Module}mm,\r\n";
            Information += $"pressure angle = {PressureAngle * 180 / Math.PI:N1}\u00b0, precision = {MaxError}mm\r\n";
            Information += $"backlash = {Backlash * Module}mm, cutter diameter = {CutDiameter}mm\r\n";
            Information += $"contact ratio = {(pinionAddendumAngle + pinionDedendumAngle) / ToothAngle:N3}\r\n";
            Information += $"max pressure angles: {180 / Math.PI * maxPinionPressureAngle:N2}/{180 / Math.PI * maxWheelPressureAngle:N2}\r\n";
        }

        public string ShortName
            => $"Ct{ToothCount}o{OpposingToothCount}m{Module:N2}a{PressureAngle * 180 / Math.PI:N1}"
                + $"e{MaxError:N2}b{Backlash * Module:N2}c{CutDiameter:N2}.svg";

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

        public double PressureAngle { get; private set; }

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
        private double wheelDedendumRadius;
        private double maxPinionPressureAngle;
        private double maxWheelPressureAngle;
        private IList<Coordinate> oneToothProfile;

        public double AddendumDiameter => pinionAddendumRadius * 2;

        private double MaxAngleFunc(double rp, double rk, double phi, double t)
        {
            // First calculate the position on the epicycloid after the locus
            // wheel has rolled round the pinion by phi radians

            double c = rp + rk;
            double d = phi * c / rk;
            double sinpt = Math.Sin(phi + t);
            double sindt = Math.Sin(d + t);
            return rk * sindt - c * sinpt;
        }

        private double CalcMaxAddendumAngle(double radius, double otherLocusRadius, int toothCount, double blunting)
        {
            double maxTipAngle = Math.Min(Math.PI * otherLocusRadius / radius, Math.PI);
            Func<double, double> func = 
                phi => MaxAngleFunc(radius, otherLocusRadius, phi, -0.5 * (1 - blunting) * Math.PI / toothCount);
            return Geometry.RootBinarySearch(func, 0, maxTipAngle, Math.PI / 2048); // Approx 0.1 degree resolution
        }

        /// <summary>
        /// For a given maximum pressure angle, find the angles subtended at the wheel
        /// and pinion centres between the contact point of the two pitch circles and
        /// the upper and lower points at which the contact loci angles begin to exceed
        /// the requested pressure angle. Also compute the minimum addendum radius
        /// for each gear to reach this pressure angle.
        /// </summary>
        /// <param name="maxPressureAngle">The desired maximum pressure angle between
        /// teeth. Note that for cycloidal gears, the pressure angle varies between
        /// zero when contact between gears is on the line of centres, and higher
        /// values the further either side of that point the wheels turn. These
        /// loci are arcs of circles that lie on the circles used to create the
        /// epicycloids and hypocycloids that make up the gear teeth shapes.</param>
        /// <param name="locusRadius">The radius of the pinion locus circle. The
        /// pinion is the gear with ToothCount teeth</param>
        /// <param name="wheelLocusRadius">The radius of the wheel locus circle.
        /// The wheel is the opposite gear our gear will be engaging with.</param>
        /// <param name="pressureAngle">The trial maximum pressure angle
        /// we shall calculate for</param>
        
        private void CalcMinimumCriteria
            (double locusRadius, double wheelLocusRadius, double pressureAngle)
        {
            // First calculate the minimum criteria based on maximum
            // pressure angle between contact points on the two gears

            var sinPA = Math.Sin(pressureAngle);
            var cosPA = Math.Cos(pressureAngle);
            var tls = 2 * locusRadius * sinPA;
            double pinionRadius = PitchRadius;
            double wheelRadius = OpposingToothCount * Module / 2;
            double centres = pinionRadius + wheelRadius;
            double contactY= tls * cosPA;
            double contactX = tls * sinPA;
            pinionDedendumAngle = Math.Atan2(contactY, pinionRadius - contactX);
            wheelAddendumAngle = Math.Atan2(contactY,  wheelRadius + contactX);
            wheelAddendumRadius = Math.Sqrt
                (Coordinate.SumOfSquares(contactY, wheelRadius + contactX));
            pinionDedendumRadius = centres - wheelAddendumRadius;
            tls = 2 * wheelLocusRadius * sinPA;
            contactY = tls * cosPA;
            contactX = tls * sinPA;
            wheelDedendumAngle = Math.Atan2(contactY, wheelRadius - contactX);
            pinionAddendumAngle = Math.Atan2(contactY, pinionRadius + contactX);
            pinionAddendumRadius = Math.Sqrt
                (Coordinate.SumOfSquares(contactY, pinionRadius + contactX));
            wheelDedendumRadius = centres - pinionAddendumRadius;
        }

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
            wheelDedendumRadius = centres - pinionAddendumRadius;
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
        private string InitPointLists()
        {
            // Assume clock toothing with locii radius half of wheel radius

            CalcMaximumCriteria(ToothCount * Module / 4, 
                OpposingToothCount * Module / 4, ToothBlunting, OpposingToothBlunting);
            
            // Validate that the angles can support the number of teeth suggested

            var wheelToothAngle = 2 * Math.PI / OpposingToothCount;
            if (PinionContactRatio < 1)
                return "Gear needs more teeth for contact ratio >= 1.0";
            if(WheelContactRatio < 1)
                return "Opposite gear needs more teeth for contact ratio >= 1.0";
            oneToothProfile = OneToothProfile();
            if (oneToothProfile == null)
                return "Unable to form the profile for each tooth of this gear";
            return String.Empty;
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
            Coordinate cycloidPt = new (PitchRadius, 0);
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
            Coordinate cycloidPt = new (PitchRadius, 0);
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
            List<Coordinate> cycloids = new (HypocycloidPoints()
                .Reverse()
                .Concat(EpicycloidPoints()));
            double boundaryAngle = cycloids[0].Phase;
            // Start with the dedendum circle
            // points before the rising cycloids
            List<Coordinate> toothProfile = new (Geometry.CirclePoints
                (-ToothAngle / 2 - boundaryAngle, boundaryAngle, 
                Geometry.AngleStep, pinionDedendumRadius));
            boundaryAngle = cycloids.Last().Phase;
            List<Coordinate> addendumCircle = new(Geometry.CirclePoints
                (boundaryAngle, ToothAngle / 2 - boundaryAngle, 
                Geometry.AngleStep, pinionAddendumRadius));
            toothProfile.AddRange(cycloids);
            toothProfile.AddRange(addendumCircle);
            toothProfile.AddRange(cycloids
                .Select(p => p.Conjugate.Rotate(ToothAngle/2))
                .Reverse());
            return Geometry.LinearReduction(toothProfile, MaxError);
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
    }
}
