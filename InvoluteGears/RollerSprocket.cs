using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
    public class RollerSprocket : IGearProfile
    {
        public RollerSprocket(int teeth, double pitch, double err, double roller, double backlash, double inner, double cutDiameter)
        {
            ToothCount = teeth;
            Pitch = pitch;
            MaxError = err;
            RollerDiameter = roller;
            Backlash = backlash;
            ChainWidth = inner;
            CutDiameter = cutDiameter;
            SetInformation();
            var oneTooth = CalculateOneTooth();
            OuterToothProfile = Involutes.LinearReduction
                (oneTooth, (float)MaxError);
        }

        private readonly List<PointF> OuterToothProfile = null;

        public string ShortName
            => $"RSt{ToothCount}p{Pitch:N2}e{MaxError:N2}r{RollerDiameter:N2}b{Backlash:N2}w{ChainWidth:N2}.svg";

        private void SetInformation()
        {
            Information = $"Roller sprocket: {ToothCount} teeth, pitch = {Pitch}mm\r\n";
            Information += $"precision = {MaxError}mm, roller dia = {RollerDiameter}mm\r\n";
            Information += $"backlash = {Backlash}mm, side plate = {ChainWidth:N2}\r\n";
        }

        public string Information { get; private set; }

        /// <summary>
        /// A count of the number of chain links around the sprocket.
        /// </summary>

        public int ToothCount { get; private set; }

        /// <summary>
        /// The linear distance between adjacent roller centres
        /// </summary>

        public double Pitch { get; private set; }

        /// <summary>
        /// The diameter of the rollers in the chain
        /// </summary>

        public double RollerDiameter { get; private set; }

        /// <summary>
        /// The desired slack between the rollers and the sprocket
        /// </summary>

        public double Backlash { get; private set; }

        /// <summary>
        /// Module is not used by roller sprockets as the gear diameter
        /// is calculated from the pitch between rollers. It is however
        /// in the interface, so is provided here as the diameter across
        /// opposite roller centres divided by the number of teeth on
        /// the sprocket.
        /// </summary>

        public double Module =>
            2 * PitchRadius / ToothCount;

        /// <summary>
        /// The tolerance in precision for cutting the sprocket
        /// </summary>

        public double MaxError { get; private set; }

        /// <summary>
        /// The maximum width of the chain side plates. Used to
        /// calculate how much the edge of the chain would overlap
        /// the rollers.
        /// </summary>

        public double ChainWidth { get; private set; }

        /// <summary>
        /// The diameter of the cutter bit used to cut out
        /// the sprocket shape
        /// </summary>

        public double CutDiameter { get; private set; }

        /// <summary>
        /// The diameter of the base circle on which the chain edges
        /// rest when wrapped around the sprocket
        /// </summary>

        public double InnerDiameter =>
            2 * PitchRadius - ChainWidth;

        /// <summary>
        /// Radius of sprocket to centre of chain roller
        /// </summary>

        public double PitchRadius =>
            Pitch / (2 * Math.Sin(Math.PI / ToothCount));

        /// <summary>
        /// The dimension for the size of the rendered image
        /// </summary>

        public double OuterDiameter { get; private set; }

        private List<PointF> CalculateOneTooth()
        {
            // Adjust roller diameter for required backlash

            double rollerRadius = (RollerDiameter + Backlash) / 2;
            if (rollerRadius < CutDiameter / 2)
            {
                Information += "Roller radius too small for cutter diameter\r\n";
                return new();
            }

            // Find the centre of the upper roller

            double jy = Pitch / 2;
            double jx = jy / Math.Tan(Math.PI / ToothCount);

            // Calculate the contact part of the roller with
            // the sprocket when embedded in the sprocket

            var contactPts = Involutes.CirclePoints(
                Math.PI * (1 + 1.0 / ToothCount),
                Math.PI * 1.5,
                Involutes.AngleStep,
                rollerRadius,
                Involutes.CreatePt(jx, jy));

            // The angle from the roller centre to the tip of
            // the tooth at the sprocket's maximum radius

            var addendumAngle = AddendumAngle();
            var tipAngle = ToothCurveAngle(addendumAngle);

            // Calculate the part of the tooth that clears the
            // roller while it is unwinding from the sprocket

            var clearPts = Involutes.CirclePoints(
                tipAngle,
                Math.PI / 2,
                Involutes.AngleStep,
                Pitch - rollerRadius,
                Involutes.CreatePt(jx, -jy));

            var rimPoints = Involutes.CirclePoints(
                0,
                addendumAngle,
                Involutes.AngleStep,
                PitchRadius + rollerRadius);

            // Set the dimensions of the bounding box

            OuterDiameter = (PitchRadius + rollerRadius) * 2;
            return contactPts
                .Select(p => Involutes.Conjugate(p))
                .Concat(clearPts.Reverse().Select(p => Involutes.Conjugate(p)))
                .Concat(rimPoints.Select(p => Involutes.Conjugate(p)).Reverse())
                .Concat(rimPoints)
                .Concat(clearPts)
                .Concat(contactPts.Reverse()).ToList();
        }

        private IEnumerable<PointF> ToothProfile(int gap)
        {
            double angle = 2 * Math.PI * (gap % ToothCount) / (double)ToothCount;
            return
                OuterToothProfile
                .Select(p => Involutes.RotateAboutOrigin(angle, p));
        }

        /// <summary>
        /// Generate the complete path of
        /// points for the whole sprocket outer edge
        /// </summary>
        /// <returns>The set of points describing the sprocket outer edge
        /// </returns>

        public IEnumerable<PointF> GenerateCompleteGearPath() => Enumerable
                .Range(0, ToothCount)
                .Select(i => ToothProfile(i))
                .SelectMany(p => p);

        /// <summary>
        /// Calculate the path for the base wheel either side of the teeth
        /// </summary>
        /// <returns>Base wheel circle</returns>

        public IEnumerable<PointF> GenerateInnerGearPath() =>
            Involutes.CirclePoints
                (0, 2 * Math.PI, Involutes.AngleStep, InnerDiameter / 2);

        /// <summary>
        /// Calculate the angle between the X axis and the point at which the
        /// addendum circle meets the tooth edge. This enables us to cut off
        /// the teeth rather than have a long sharp point sticking out past
        /// the outer edge of the roller chain.
        /// </summary>
        /// <returns>The angle between the X axis and the point at which
        /// the addendum circle meets the tooth edge</returns>

        private double AddendumAngle()
        {
            var rollerRadius = (RollerDiameter + Backlash) / 2;
            var addendumRadius = PitchRadius + rollerRadius;
            var toothRadius = Pitch - rollerRadius;
            var cosCentreAngle = PitchRadius * PitchRadius
                + addendumRadius * addendumRadius
                - toothRadius * toothRadius;
            cosCentreAngle /= 2 * PitchRadius * addendumRadius;
            return Math.Acos(cosCentreAngle) - Math.PI / ToothCount;
        }

        /// <summary>
        /// Obtain the angle formed from the centre of the lower roller
        /// to the point at which the tooth curve meets the addendum circle
        /// </summary>
        /// <param name="addendumAngle">The addendum angle as computed
        /// by calling AddendumAngle()</param>
        /// <returns>The angle to the interssection point</returns>

        private double ToothCurveAngle(double addendumAngle)
        {
            // Find the centre of the lower roller

            var rx = Math.Sqrt(PitchRadius * PitchRadius - Pitch * Pitch / 4);
            var ry = -Pitch / 2;

            // Find the intersection point coordinates

            var rollerRadius = (RollerDiameter + Backlash) / 2;
            var mx = (PitchRadius + rollerRadius) * Math.Cos(addendumAngle);
            var my = (PitchRadius + rollerRadius) * Math.Sin(addendumAngle);

            // Calculate the angle from (rx, ry) to (mx, my)

            return Math.Atan2(my - ry, mx - rx);
        }
    }
}
