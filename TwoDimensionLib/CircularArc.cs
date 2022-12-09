using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib;

/// <summary>
/// Unlike the arc used in SVG files, this is
/// a fragment of a circle. Note that the radius
/// is determined by the starting point's
/// distance from the centre.
/// </summary>
public class CircularArc : IDrawable
{
    public Coordinate Centre { get; set; }

    public double StartAngle { get; set; }
    
    public double EndAngle { get; set; }

    public double Radius { get; set; }

    public bool Anticlockwise { get; set; }

    public Coordinate Start 
        => Centre + Coordinate.FromPolar(Radius, StartAngle);

    public Coordinate End
        => Centre + Coordinate.FromPolar(Radius, EndAngle);

    public Rectangle Bounds
    {
        get
        {
            BoundsTracker b = new BoundsTracker();
            b.Track(Start);
            b.Track(End);
            double sa = Anticlockwise ? StartAngle : EndAngle;
            double ea = Anticlockwise ? EndAngle : StartAngle;
            sa = Geometry.NormaliseAngle(sa);
            ea = Geometry.NormaliseAngle(ea);
            if (sa < Math.PI / 2 && ea >= Math.PI / 2)
                b.Track(Centre + new Coordinate(0, Radius));
            if (sa < Math.PI && ea >= Math.PI)
                b.Track(Centre + new Coordinate(-Radius, 0));
            if(sa < 1.5 * Math.PI && ea >= 1.5 * Math.PI)
                b.Track(Centre + new Coordinate(0, -Radius));
            if (sa <= 0 && ea > 0)
                b.Track(Centre + new Coordinate(Radius, 0));
            return b.Bounds;
        }
    }

    public IDrawable Reversed()
        => new CircularArc
        {
            Centre = this.Centre,
            StartAngle = this.EndAngle,
            EndAngle = this.StartAngle,
            Radius = this.Radius,
            Anticlockwise = !this.Anticlockwise
        };

    public IDrawable ReflectY()
        => new CircularArc
        {
            Centre = this.Centre.Conjugate,
            StartAngle = -this.StartAngle,
            EndAngle = -this.EndAngle,
            Radius = this.Radius,
            Anticlockwise = !this.Anticlockwise
        };

    public IDrawable RotatedBy(double phi, Coordinate pivot)
        => new CircularArc
        {
            Centre = Centre.RotateAbout(pivot, phi),
            StartAngle = this.StartAngle + phi,
            EndAngle = this.EndAngle + phi,
            Radius = this.Radius,
            Anticlockwise = this.Anticlockwise
        };

    public IDrawable Translated(Coordinate offset) 
        => new CircularArc
        {
            Centre = Centre.Offset(offset),
            StartAngle = this.StartAngle,
            EndAngle = this.EndAngle,
            Radius = this.Radius,
            Anticlockwise = this.Anticlockwise
        };
}