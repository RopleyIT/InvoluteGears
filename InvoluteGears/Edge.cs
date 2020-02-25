using System;
using System.Collections.Generic;
using System.Text;

namespace InvoluteGears
{
    public class Edge
    {
        Vertex start;
        Vertex end;

        public Edge(Vertex v1, Vertex v2)
        {
            start = v1;
            end = v2;
        }

        public double Gradient()
        {
            double dx = end.X - start.X;
            if (dx == 0)
                return double.PositiveInfinity;
            else
                return (end.Y - start.Y) / dx;
        }

        public double CrossesY()
        {
            double g = Gradient();
            if (g == double.PositiveInfinity)
                return double.PositiveInfinity;
            else
                return start.Y - Gradient() * start.X;
        }
        
        public static bool Between(double v1, double mid, double v2)
        {
            if (mid < v2 && mid >= v1)
                return true;
            if (mid < v1 && mid >= v2)
                return true;
            return false;
        }

        public Vertex Start => start;
        public Vertex End => end;

        public Vertex? Intersection(Edge other)
        {
            // Get the y = mx + c coefficients
            // of the two straight lines

            var m1 = Gradient();
            var m2 = other.Gradient();
            var c1 = CrossesY();
            var c2 = other.CrossesY();

            // Parallel lines will never cross, so
            // return a null vertex to show this

            if (m1 == m2)
                return null;

            // Calculate the X value for the intersection.
            // Note that vertical lines will have an infinite
            // gradient, so are handled as special cases

            double x;
            if (m1 == double.PositiveInfinity)
                x = start.X;
            else if (m2 == double.PositiveInfinity)
                x = other.start.X;
            else
                x = (c1 - c2) / (m2 - m1);

            // Now use either of the two lines to
            // calculate the Y value of the intersection

            double y = m1 * x + c1;

            // This intersection at X, Y assumes the lines
            // are of infinite length. They are however
            // bounded, so we need a further test to see
            // if the intersection lies between min and max
            // for each line

            if (Between(start.X, x, end.X) 
                && Between(other.start.X, x, other.end.X))
                return new Vertex { X = x, Y = y };
            else
                return null;
        }

        public double? CalcY(double x)
        {
            Edge vertical = new Edge(
                new Vertex { X = x, Y = start.Y },
                new Vertex { X = x, Y = end.Y });
            Vertex? intersection = Intersection(vertical);
            if (intersection.HasValue)
                return intersection.Value.Y;
            else
                return null;
        }

        public double? CalcX(double y)
        {
            Edge horizontal = new Edge(
                new Vertex { X = start.X, Y = y },
                new Vertex { X = end.X, Y = y });
            Vertex? intersection = Intersection(horizontal);
            if (intersection.HasValue)
                return intersection.Value.X;
            else
                return null;
        }
    }
}
