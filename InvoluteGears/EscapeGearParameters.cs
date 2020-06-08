using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Schema;

namespace InvoluteGears
{
    /// <summary>
    /// Describe the shape of an escape wheel
    /// with inclined sharp teeth.
    /// </summary>
    
    public class EscapeGearParameters : IGearProfile
    {
        public EscapeGearParameters(
            int toothCount, 
            double module, 
            double undercutAngle,
            double toothFaceLength,
            double tipPitch,
            double cutDiameter,
            double maxErr)
        {
            ToothCount = toothCount;
            Module = module;
            UndercutAngle = undercutAngle;
            ToothFaceLength = toothFaceLength;
            TipPitch = tipPitch;
            CutDiameter = cutDiameter;
            MaxError = maxErr;
            SetInformation();
            CalculatePoints();
        }
        private void SetInformation()
        {
            Information = $"{ToothCount} teeth, module = {Module}mm, undercut angle = {UndercutAngle * 180 / Math.PI:N1}\u00b0\r\n";
            Information += $"tooth face = {ToothFaceLength}mm, precision = {MaxError}mm\r\n";
            Information += $"tip width = {TipPitch}mm, tooth gap diameter = {CutDiameter}mm\r\n";
        }
        public string ShortName
            => $"t{ToothCount}m{Module:N2}u{UndercutAngle * 180 / Math.PI:N1}f{ToothFaceLength:N2}"
                + $"e{MaxError:N2}p{TipPitch:N2}c{CutDiameter:N2}.svg";

        /// <summary>
        /// Used for warning or information messages when methods invoked
        /// </summary>

        public string Information { get; private set; }

        /// <summary>
        /// The accuracy of the points in the profile
        /// </summary>

        public double MaxError { get; private set; }

        /// <summary>
        /// The diameter of the curved part of the
        /// cut into the gear. Must be larger than
        /// the bit used to cut out the gear.
        /// </summary>

        public double CutDiameter
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of teeth around the outside of the
        /// gear wheel. These are evenly spaced obviously.
        /// </summary>

        public int ToothCount { get; private set; }

        /// <summary>
        /// The angle between the radial from the centre of
        /// the gear and the surface of the tooth end. Note
        /// that unlike the pressure angle of an involute
        /// gear, this angle is the undercut angle as the
        /// escape wheel teeth are oblique.
        /// </summary>

        public double UndercutAngle { get; private set; }

        /// <summary>
        /// The length of the flat face of a tooth from
        /// the tooth tip to the point at which it curves
        /// back for the next tooth trailing edge.
        /// </summary>

        public double ToothFaceLength { get; private set; }

        /// <summary>
        /// The extra diameter needed per extra tooth for a
        /// given tooth width. In practice this value is often
        /// used to determine pitch circle diameter and tooth
        /// separation, rather than the other way round. 
        /// Units in mm.
        /// </summary>

        public double Module { get; private set; }

        /// <summary>
        /// The distance round the pitch circle for
        /// the thickness of the tooth tip.
        /// </summary>

        public double TipPitch { get; private set; }

        // --- DERIVED PROPERTIES --- //

        /// <summary>
        /// The pitch circle diameter for an escape
        /// wheel is its true diameter to the ends of
        /// the teeth.
        /// </summary>

        public double PitchCircleDiameter => Module * ToothCount;

        /// <summary>
        /// The distance between adjacent teeth around the pitch circle
        /// </summary>

        public double Pitch
            => Math.PI * Module;

        /// <summary>
        /// The angle occupied by one tooth and one gap
        /// </summary>

        public double ToothAngle => 2 * Math.PI / ToothCount;

        /// <summary>
        /// The angle at the centre of the gear subtended by
        /// the width of the tip of a tooth.
        /// </summary>
        
        public double TipAngle => 2 * TipPitch / (ToothCount * Module);

        /// <summary>
        /// The angle at the centre of the gear subtended by
        /// the width of the gap between two teeth.
        /// </summary>

        public double GapAngle => ToothAngle - TipAngle;

        private PointF ToothTip;
        private PointF UnderCutCentre;
        private PointF FaceEnd;
        private PointF BackTip;
        private double BackAngle;
        private List<PointF> OneToothProfile;

        private void CalculatePoints()
        {
            // Unrotated tooth tip is on the X axis

            ToothTip = Involutes.CreatePt(PitchCircleDiameter / 2, 0);

            // Navigate down the slope of the tooth face

            FaceEnd = Involutes.CreatePt(
                PitchCircleDiameter / 2 - ToothFaceLength * Math.Cos(UndercutAngle),
                -ToothFaceLength * Math.Sin(UndercutAngle));

            // Now find the centre of the undercut circle

            UnderCutCentre = Involutes.CreatePt
                (FaceEnd.X - CutDiameter * Math.Sin(UndercutAngle) / 2,
                FaceEnd.Y + CutDiameter * Math.Cos(UndercutAngle) / 2);

            // Find the coordinates of the back tip corner of
            // the next tooth in an anticlockwise direction

            BackTip = Involutes.CreatePt(ToothTip.X * Math.Cos(GapAngle),
                ToothTip.X * Math.Sin(GapAngle));

            // Find the coordinates of the tangent to the
            // undercut circle of a line drawn from the
            // back of the tooth tip

            var tipToEndAngle = 
                Math.Asin(CutDiameter / (2 * Involutes.DistanceBetween(BackTip, UnderCutCentre)));
            var tiptoCtrAngle = 
                Math.Atan2(BackTip.Y - UnderCutCentre.Y, BackTip.X - UnderCutCentre.X);
            BackAngle = 
                tiptoCtrAngle + Math.PI / 2 - tipToEndAngle;
            OneToothProfile = ComputeOnePitch();
        }

        private List<PointF> ComputeOnePitch()
        {
            var points = new List<PointF>
            {
                ToothTip,
                FaceEnd
            };
            var startAngle = -Math.PI / 2 + UndercutAngle;
            points.AddRange(
                Involutes.CirclePoints(
                    BackAngle, 2*Math.PI + startAngle, Involutes.AngleStep, 
                    CutDiameter / 2, UnderCutCentre)
                .Reverse());
            points.Add(BackTip);
            points.AddRange(
                Involutes.CirclePoints(
                    GapAngle, ToothAngle, Involutes.AngleStep, 
                    PitchCircleDiameter / 2));
            return Involutes.LinearReduction(points, (float)MaxError);
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

        public IEnumerable<PointF> ToothProfile(int gap) 
            => Involutes.RotateAboutOrigin
                ((gap % ToothCount) * ToothAngle, OneToothProfile);

        /// <summary>
        /// Generate the complete path of
        /// points for the whole  escape wheel
        /// </summary>
        /// <returns>The set of points describing the escape wheel
        /// </returns>

        public IEnumerable<PointF> GenerateCompleteGearPath()
        {
            return Enumerable
                .Range(0, ToothCount)
                .Select(i => ToothProfile(i))
                .SelectMany(p => p);
        }

        /// <summary>
        /// The distance from the centre of the gear to the
        /// closest part of the curve between leading and
        /// trailing faces of the teeth.
        /// </summary>

        public double InnerDiameter 
            => 2 * Involutes.DistanceBetween(UnderCutCentre, PointF.Empty) - CutDiameter;
    }
}
