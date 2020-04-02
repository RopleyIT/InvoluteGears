using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace Plotter
{
    /*
     <?xml version="1.0" encoding="UTF-8" standalone="no"?>
        <svg width="391" height="391" viewBox="-70.5 -70.5 391 391" xmlns="http://www.w3.org/2000/svg">
        <rect fill="#fff" stroke="#000" x="-70" y="-70" width="390" height="390"/>
        <g opacity="0.8">
	        <rect x="25" y="25" width="200" height="200" fill="green" stroke-width="4" stroke="pink" />
	        <circle cx="125" cy="125" r="75" fill="orange" />
	        <polyline points="50,150 50,200 200,200 200,100" stroke="red" stroke-width="4" fill="none" />
	        <line x1="50" y1="50" x2="200" y2="200" stroke="blue" stroke-width="4" />
        </g>
        </svg>
     */
    public class SVGCreator
    {
        private string filePath;

        public SVGCreator(string path)
        {
            filePath = path;
        }

        public SizeF DocumentDimensions { get; set; }
        
        public string DocumentDimensionUnits { get; set; }

        public RectangleF ViewBoxDimensions { get; set; }

        public string ViewBoxDimensionUnits { get; set; }

        string XmlHeader => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>";
        
        string StartSvg =>
            $"<svg width=\"{DocumentDimensions.Width}{DocumentDimensionUnits}\" "
            + $"height=\"{DocumentDimensions.Height}{DocumentDimensionUnits}\" "
            + $"viewBox=\"{ViewBoxDimensions.X}{ViewBoxDimensionUnits} "
            + $"{ViewBoxDimensions.Y}{ViewBoxDimensionUnits} "
            + $"{ViewBoxDimensions.Width}{ViewBoxDimensionUnits} "
            + $"{ViewBoxDimensions.Height}{ViewBoxDimensionUnits}\" "
            + $"xmlns=\"http://www.w3.org/2000/svg\">";
        
        string EndSvg => "</svg>";
    }
}
