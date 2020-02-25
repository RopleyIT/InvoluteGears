using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Plotter
{
    public class Plot
    {
        double ScaleFactor(RectangleF bounds, int width, int height)
        {
            double scaleY = height / bounds.Height;
            double scaleX = width / bounds.Width;
            return Math.Min(scaleX, scaleY);
        }

        public Image PlotGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height)
        {
            Color[] colours = { Color.Black, Color.Brown, Color.Red, Color.DarkBlue,
                Color.Green, Color.Blue, Color.Purple, Color.Gray};

            Bitmap bmp = new Bitmap(width, height);
            BoundsF bounds = new BoundsF();
            List<List<PointF>> plots = new List<List<PointF>>();
            foreach (var pl in points)
                plots.Add(bounds.Track(pl).ToList());
            double scale = ScaleFactor(bounds.Bounds, width, height);
            using Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.LightGray, 0, 0, width, height);
            int index = 0;
            foreach (List<PointF> pl in plots)
                PlotGraph(pl, g, bounds.Bounds, scale, colours[index++%7]);
            return bmp;
        }

        public Image PlotGraph(List<PointF> points, int width, int height)
        {
            List<List<PointF>> pointLists = new List<List<PointF>> { points };
            return PlotGraphs(pointLists, width, height);
        }

        private static void PlotGraph(List<PointF> points, Graphics g, 
            RectangleF bounds, double scale, Color penColor)
        {
            using Pen p = new Pen(penColor, 1.0f);
            for (int i = 0; i < points.Count - 1; i++)
                g.DrawLine(p, (int)(0.5 + scale * (points[i].X - bounds.X)),
                    (int)(0.5 + scale * (points[i].Y - bounds.Y)),
                    (int)(0.5 + scale * (points[i + 1].X - bounds.X)),
                    (int)(0.5 + scale * (points[i + 1].Y - bounds.Y)));
        }
    }
}
