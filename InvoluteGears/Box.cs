using System;
using System.Collections.Generic;
using System.Text;

namespace InvoluteGears
{
    public class Box
    {
        private Vertex tlhc;
        private Vertex brhc;

        public Box(Vertex v1, Vertex v2)
        {
            tlhc.X = Math.Min(v1.X, v2.X);
            tlhc.Y = Math.Min(v1.Y, v2.Y);
            brhc.X = Math.Max(v1.X, v2.X);
            brhc.Y = Math.Max(v1.Y, v2.Y);
        }

        public Box()
        {
            tlhc = new Vertex { X = 0.0, Y = 0.0 };
            brhc = new Vertex { X = 0.0, Y = 0.0 };
        }

        public Vertex TLHC => tlhc;
        public Vertex BRHC => brhc;

        public double Top => tlhc.Y;
        public double Bottom => brhc.Y;
        public double Left => tlhc.X;
        public double Right => brhc.Y;
        public double Width => Right - Left;
        public double Height => Bottom - Top;
    }
}
