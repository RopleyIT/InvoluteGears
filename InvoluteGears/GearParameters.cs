using System;

namespace InvoluteGears
{
    public class GearParameters
    {
        // Two gears that are engaged with each other each
        // have a pitch circle. The pitch circles of the
        // two gears are tangential to each other and have
        // radii that are in the same ratio as the gear
        // ratio between the two gears.

        public double PitchCircleDiameter => Module * ToothCount;

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
            => Math.PI * Module * Math.Cos(PressureAngle);

        /// <summary>
        /// The angle from the centre of a normal width involute tooth to the
        /// point on the base circle at which its involute touches.
        /// At the pitch circle two teeth are in contact at the
        /// pressure angle. Two gears 'roll' on each other at the
        /// pitch circle, and the ratio between two pitch circle
        /// radii is used to derive the gear ratio. This point is
        /// Pi/(2*ToothCount) radians from the tooth centre. It is
        /// also PressureAngle radians from the point at which
        /// its involute through this contact point touches the
        /// base circle. Finally if the line from that touch point
        /// were wrapped around the outside of the base circle,
        /// the point at which it ends would be at the angle we
        /// are interested in.
        /// </summary>

        public double ToothBaseOffset
            => Math.Tan(PressureAngle) - PressureAngle + Math.PI / (2 * ToothCount);
    }
}
