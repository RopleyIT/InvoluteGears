using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
    public class ClosedShape
    {
        public List<Vertex> Vertices
        {
            get;
            set;
        } = new List<Vertex>();

        public IEnumerable<Edge> Edges
        {
            get
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertex earlier = Vertices[i];
                    Vertex later = Vertices[(i + 1) % Vertices.Count];
                    yield return new Edge(earlier, later);
                }
            }
        }

        public Box Bounds()
        {
            Vertex tlhc = new Vertex
            {
                X = Vertices.Select(v => v.X).Min(),
                Y = Vertices.Select(v => v.Y).Min()
            };
            Vertex brhc = new Vertex
            {
                X = Vertices.Select(v => v.X).Max(),
                Y = Vertices.Select(v => v.Y).Max()
            };
            return new Box(tlhc, brhc);
        }

        public bool Encloses(Vertex v)
        {
            // Apply the crossings rule to
            // determine if the point is 
            // within the closed shape

            // Quick check on bounding box as this
            // gives quick answers if v is outside

            Box bounds = Bounds();
            if (v.X < bounds.Left || v.X >= bounds.Right)
                return false;
            if (v.Y < bounds.Top || v.Y >= bounds.Bottom)
                return false;

            // Now apply the line crossing rule. Find all
            // edges of the polygon that cross a line drawn 
            // parallel to the X axis from the vertex.

            var yBetweens = Edges.Where
                (e => Edge.Between(e.Start.Y, v.Y, e.End.Y));

            var edgesToRight = yBetweens
                .Where(e => e.CalcX(v.Y).Value > v.X)
                .Count();
            return edgesToRight % 2 != 0;
        }

    }
}
