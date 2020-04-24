﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            g.FillRectangle(Brushes.White, 0, 0, width, height);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            PlotAxes(bounds, scale, bmp);
            int index = 0;
            foreach (List<PointF> pl in plots)
                PlotGraph(pl, g, bounds.Bounds, scale, colours[index++%7]);
            return bmp;
        }

        private void PlotAxes(BoundsF bounds, double scale, Bitmap bmp)
        {
            double unitsX = UnitSize(bounds.Bounds.Width);
            double unitsY = UnitSize(bounds.Bounds.Height);

            using Graphics g = Graphics.FromImage(bmp);
            for (double v = RoundUp(bounds.Bounds.X, unitsX); v < bounds.Bounds.Right; v += unitsX)
            {
                List<PointF> rule = new List<PointF>();
                rule.Add(new PointF { X = (float)v, Y = bounds.Bounds.Y });
                rule.Add(new PointF { X = (float)v, Y = bounds.Bounds.Bottom });
                PlotGraph(rule, g, bounds.Bounds, scale, Color.Gray);
                LabelXRule(v, g, bounds, scale);
            }
            for (double v = RoundUp(bounds.Bounds.Y, unitsY); v < bounds.Bounds.Bottom; v += unitsY)
            {
                List<PointF> rule = new List<PointF>();
                rule.Add(new PointF { Y = (float)v, X = bounds.Bounds.X });
                rule.Add(new PointF { Y = (float)v, X = bounds.Bounds.Right });
                PlotGraph(rule, g, bounds.Bounds, scale, Color.Gray);
                LabelYRule(v, g, bounds, scale);
            }
        }

        private void LabelXRule(double v, Graphics g, BoundsF bounds, double scale)
        {
            // First generate label string

            string label = v.ToString("G2");
            Font font = new Font("Consolas", 30F);
            var txtSize = g.MeasureString(label, font);

            // Find the line position

            float x = (float)(0.5 + scale * (v - bounds.Bounds.X));
            x -= txtSize.Width / 2;
            g.FillRectangle(Brushes.White, x, 0, txtSize.Width, txtSize.Height);
            g.DrawString(label, font, Brushes.Gray, x, 0);
        }

        private void LabelYRule(double v, Graphics g, BoundsF bounds, double scale)
        {
            // First generate label string

            string label = v.ToString("G3");
            Font font = new Font("Consolas", 30F);
            var txtSize = g.MeasureString(label, font);

            // Find the line position

            float y = (float)(0.5 + scale * (v - bounds.Bounds.Y));
            y -= txtSize.Height / 2;
            if (y > txtSize.Height * 1.5)
            {
                g.FillRectangle(Brushes.White, 0, y, txtSize.Width, txtSize.Height);
                g.DrawString(label, font, Brushes.Gray, 0, y);
            }
        }

        private double RoundUp(double x, double unitsX) => Math.Ceiling(x / unitsX) * unitsX;
        private double UnitSize(double range)
        {
            double log = Math.Log10(range);
            double exponent = Math.Floor(log);
            double baseUnit = Math.Pow(10.0, exponent - 1);
            double mantissa = log - exponent;
            if (mantissa <= Math.Log10(2))
                return 2 * baseUnit;
            if (mantissa <= Math.Log10(5))
                return 5 * baseUnit;
            return 10 * baseUnit;
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