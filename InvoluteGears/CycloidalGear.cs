using System;
using System.Collections.Generic;
using System.Linq;
using TwoDimensionLib;

namespace InvoluteGears
{
    public class CycloidalGear : IGearProfile
    {
        public CycloidalGear(int toothCount, int oppositeToothCount, double module, 
            double maxErr, double pressureAngle, double backlash, double cutterDiam)
        {
            ToothCount = toothCount;
            Module = module;
            MaxError = maxErr;
            Backlash = backlash;
            PressureAngle = pressureAngle;
            OpposingToothCount = oppositeToothCount;
            CutDiameter = cutterDiam;
            SetInformation();
            Information += InitPointLists();
        }
        private void SetInformation()
        {
            Information = $"Cycloid: {ToothCount}/{OpposingToothCount} teeth, module = {Module}mm,\r\n";
            Information += $"pressure angle = {PressureAngle * 180 / Math.PI:N1}\u00b0, precision = {MaxError}mm\r\n";
            Information += $"backlash = {Backlash * Module}mm, cutter diameter = {CutDiameter}mm\r\n";
        }

        public string ShortName
            => $"Ct{ToothCount}o{OpposingToothCount}m{Module:N2}a{PressureAngle * 180 / Math.PI:N1}"
                + $"e{MaxError:N2}b{Backlash * Module:N2}c{CutDiameter:N2}.svg";

        public string Information { get; private set; }

        public int ToothCount { get; private set; }

        public double Module { get; private set; }  

        public double MaxError { get; private set; }

        public double InnerDiameter => throw new NotImplementedException();

        public double CutDiameter { get; private set; }

        public double Backlash { get; private set; }

        public int OpposingToothCount { get; private set; }

        public double PressureAngle { get; private set; }

        // The total angle for which the pinion and wheel teeth are in contact
        // at the pinion is pinionUpperAngle + pinionLowerAngle. The point between
        // these two angles lies on the line of centres where the two pitch
        // circles touch each other. At the wheel, the angle is different because
        // the module of the wheel is different. This is the sum of wheelUpperAngle
        // and wheelLowerAngle.

        private double pinionUpperAngle;
        private double pinionLowerAngle;
        private double wheelUpperAngle;
        private double wheelLowerAngle;

        // The radii of the pinion and wheel out to the ends of the active parts
        // of their teeth. Ideally the teeth should curve gracefully beyond that
        // radius, but these radii are the points of furthest contact between teeth.

        private double pinionAddendumRadius;
        private double wheelAddendumRadius;
        private double pinionDedendumRadius;
        //private double wheelDedendumRadius;
        private IList<Coordinate> oneToothProfile;

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
        
        private void CalcMinimumCriteria
            (double locusRadius, double wheelLocusRadius)
        {
            var sinPA = Math.Sin(PressureAngle);
            var cosPA = Math.Cos(PressureAngle);
            var tls = 2 * locusRadius * sinPA;
            double pinionRadius = ToothCount * Module / 2;
            double wheelRadius = OpposingToothCount * Module / 2;
            pinionUpperAngle = Math.Atan2(tls * cosPA, pinionRadius - tls * sinPA);
            wheelUpperAngle = Math.Atan2(tls * cosPA,  wheelRadius + tls * sinPA);
            wheelAddendumRadius = Math.Sqrt
                (Coordinate.SumOfSquares(tls * cosPA, wheelRadius + tls * sinPA));
            pinionDedendumRadius = wheelRadius + pinionRadius - wheelAddendumRadius;
            tls = 2 * wheelLocusRadius * sinPA;
            pinionLowerAngle = Math.Atan2(tls * cosPA, wheelRadius - tls * sinPA);
            wheelLowerAngle = Math.Atan2(tls * cosPA, pinionRadius + tls * sinPA);
            pinionAddendumRadius = Math.Sqrt
                (Coordinate.SumOfSquares(tls * cosPA, pinionRadius + tls * sinPA));
            //wheelDedendumRadius = wheelRadius + pinionRadius - pinionAddendumRadius;
        }

        /// <summary>
        /// The average number of teeth engaged at any one time. Should
        /// exceed unity for the gear to work correctly.
        /// </summary>
        
        public double ContactRatio =>
            (pinionUpperAngle + pinionLowerAngle) / ToothAngle;

        /// <summary>
        /// The angle in radians taken up by one tooth and one dedendum
        /// </summary>
        
        public double ToothAngle => 2 * Math.PI / ToothCount;

        private string InitPointLists()
        {
            // Assume clock toothing with locii radius half of wheel radius

            CalcMinimumCriteria(ToothCount * Module / 4, OpposingToothCount * Module / 4);

            // Validate that the angles can support the number of teeth suggested

            var wheelToothAngle = 2 * Math.PI / OpposingToothCount;
            if (pinionUpperAngle + pinionLowerAngle < ToothAngle)
                return "Gear needs more teeth to support the requested pressure angle";
            if(wheelUpperAngle + wheelLowerAngle < wheelToothAngle)
                return "Opposite gear needs more teeth to support the requested pressure angle";
            oneToothProfile = OneToothProfile();
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
            double radius = 0;
            double angle = 0;
            while (angle < ToothAngle / 4 && radius < pinionAddendumRadius)
            {
                Coordinate cycloidPt = Geometry.Epicycloid
                    (ToothCount * Module / 2, OpposingToothCount * Module / 4, angle);
                radius = cycloidPt.Magnitude;
                if (radius > pinionAddendumRadius)
                    break;
                else
                    yield return cycloidPt;
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
            double radius = 0;
            double angle = 0;
            while (angle < ToothAngle / 4 && radius < pinionAddendumRadius)
            {
                Coordinate cycloidPt = Geometry.Hypocycloid
                    (ToothCount * Module / 2, OpposingToothCount * Module / 4, angle);
                radius = cycloidPt.Magnitude;
                if (radius < pinionDedendumRadius)
                    break;
                else
                    yield return cycloidPt.Conjugate;
            }
        }

        private IList<Coordinate> OneToothProfile()
        {
            List<Coordinate> cycloids = new (HypocycloidPoints()
                .Reverse()
                .Concat(EpicycloidPoints()));
            double boundaryAngle = cycloids[0].Phase;
            List<Coordinate> dedendumCircle = new (Geometry.CirclePoints
                (-ToothAngle / 2 - boundaryAngle, boundaryAngle, 
                Geometry.AngleStep, pinionDedendumRadius));
            boundaryAngle = cycloids.Last().Phase;
            List<Coordinate> addendumCircle = new(Geometry.CirclePoints
                (boundaryAngle, ToothAngle / 2 - boundaryAngle, 
                Geometry.AngleStep, pinionAddendumRadius));
            dedendumCircle.AddRange(cycloids);
            dedendumCircle.AddRange(addendumCircle);
            dedendumCircle.AddRange(cycloids.AsEnumerable().Reverse());
            return dedendumCircle;
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

        public IEnumerable<Coordinate> GenerateCompleteGearPath() => Enumerable
                .Range(0, ToothCount)
                .Select(i => ToothProfile(i))
                .SelectMany(p => p);
    }
}
