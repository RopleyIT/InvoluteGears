using System;
using System.Collections.Generic;
using System.Drawing;

namespace InvoluteGears
{
    public class GearParameters
    {
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
            => PitchCircleDiameter + 2 * Module;

        // The maximum non-interfering radius of the inner
        // circle at the foot of the gap between teeth.
        public double DedendumCircleDiameter 
            => PitchCircleDiameter - 2 * Module;

        // The number of teeth around the outside of the
        // gear wheel. These are evenly spaced obviously.
        // Note that if this is positive, it is a normal
        // gear. If zero, it is a rack. If negative, it
        // is an internal gear.
        
        public int ToothCount
        {
            get;
            set;
        }

        // The angle relative to a line between the centres
        // of two gears at which the gears touch each other.
        // By convention this is chosen to be 14.5, 20, or 25
        // degrees, though there are no design reasons why
        // these specific angles have to be used.
        
        public double PressureAngle
        {
            get;
            set;
        }

        // The extra diameter needed per extra tooth for a
        // given tooth width. In practice this value is often
        // used to determine pitch circle diameter and tooth
        // separation, rather than the other way round.

        public double Module { get; set; }

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

        public IEnumerable<PointF> AntiClockwiseInvolute(int gap)
        {
            // The angles between the middles of adjacent
            // teeth in radians is 2*PI / ToothCount

            double gapCentreAngle = (gap % ToothCount) * ToothAngle;
            double involuteBaseAngle = ToothAngle/4 - ToothBaseOffset;

            int limit = AngleIndexFloor(AddendumInvoluteAngle);
            for (int i = limit; i >= 0; i--)
            {
                double angle = (i % PointsPerRotation) * 2 * Math.PI / PointsPerRotation;
                yield return Involutes.InvolutePlusOffset(BaseCircleDiameter / 2, 0, 0, angle, 
                    gapCentreAngle + involuteBaseAngle);
            }
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
            double involuteBaseAngle = ToothAngle/4 - ToothBaseOffset;

            int limit = AngleIndexFloor(-AddendumInvoluteAngle);
            for (int i = 0; i >= limit; i--)
            {
                double angle = (i % PointsPerRotation) * 2 * Math.PI / PointsPerRotation;
                yield return Involutes.InvolutePlusOffset(BaseCircleDiameter / 2, 0, 0, angle,
                    gapCentreAngle - involuteBaseAngle);
            }
        }

        /// <summary>
        /// The angle at the dedendum circle that the mating tooth tracks
        /// the dedendum circle between the corners moving away and
        /// undercutting the involute tooth sides
        /// </summary>

        public double DedendumArcAngle 
            => (Pitch - 4 * Module * Math.Tan(PressureAngle)) / PitchCircleDiameter;

        private double Square(double v) => v * v;

        private int AngleIndexFloor(double angle) 
            => (int)(angle * PointsPerRotation / (2 * Math.PI));
    }
}
