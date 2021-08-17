using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private List<PointF> OuterToothProfile = null;

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
            Pitch / (ToothCount * Math.Sin(Math.PI / ToothCount));

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
            Pitch/Math.Sin(Math.PI/ToothCount) - ChainWidth;

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

            var tipAngle = Math.Asin(jy / (Pitch - rollerRadius));

            // Calculate the part of the tooth that clears the
            // roller while it is unwinding from the sprocket

            var clearPts = Involutes.CirclePoints(
                tipAngle,
                Math.PI / 2,
                Involutes.AngleStep,
                Pitch - rollerRadius,
                Involutes.CreatePt(jx, -jy));

            // Set the dimensions of the bounding box

            OuterDiameter = clearPts.First().X * 2;

            return contactPts
                .Select(p => new PointF(p.X, -p.Y))
                .Concat(clearPts.Reverse().Select(p => new PointF(p.X, -p.Y)))
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
    }
}
