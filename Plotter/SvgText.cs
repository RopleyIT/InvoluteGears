using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TwoDimensionLib;

namespace Plotter;

public class SvgText : IRenderable
{
    public string Stroke { get; set; }
    public string StrokeWidth { get; set; }
    public string Fill { get; set; }
    public LineCap Cap { get; set; } = LineCap.None;
    public LineJoin Join { get; set; } = LineJoin.None;
    public IEnumerable<int> Dashes { get; set; }

    public void SetDashes(params int[] dashes)
        => Dashes = dashes;

    public string FontSize { get; set; } = "20px";

    // serif; sans-serif; monospace; or cursive
    public string FontName { get; set; } = "sans-serif";

    public bool Italic { get; set; } = false;

    public bool Bold { get; set; } = false;

    public const int Left = 0;
    public const int Centre = 1; // Horizontally centered
    public const int Right = 2;
    public const int Justified = 3; // Only useful for multi-line text in fixed field width
    public const int Top = 4;
    public const int UCaseTop = 8;
    public const int LCaseTop = 12;
    public const int UCaseMiddle = 16;
    public const int LCaseMiddle = 20;
    public const int BaseLine = 0;
    public const int Bottom = 24;
    private const int HAlign = 0x3;
    private const int VAlign = 0x1C;

    private int alignment = 0;

    /// <summary>
    /// Set the position of the text coordinate relative to the text
    /// </summary>

    public int Alignment
    {
        get => alignment;
        set
        {
            if (value < 0 || value > Justified + Bottom)
                throw new ArgumentException("SVG text alignment value out of range");
            else
                alignment = value;
        }
    }

    private string AlignmentAttributes()
    {
        string halign = (alignment & HAlign) switch
        {
            Left => "",
            Centre => "middle",
            Justified => "middle",
            Right => "end",
            _ => ""
        };

        string valign = (alignment & VAlign) switch
        {
            Top => "text-before-edge",
            UCaseTop => "hanging",
            LCaseTop => "mathematical",
            UCaseMiddle => "central",
            LCaseMiddle => "middle",
            BaseLine => "",
            Bottom => "text-after-edge",
            _ => ""
        };

        if (!string.IsNullOrEmpty(halign))
            halign = $" text-anchor=\"{halign}\"";
        if (!string.IsNullOrEmpty(valign))
            valign = $" dominant-baseline=\"{valign}\"";
        return halign + valign;
    }

    private readonly string text;
    private Coordinate location;

    public SvgText(string text, Coordinate btmLeft, string size, string font, bool italic, bool bold)
    {
        this.text = text;
        location = btmLeft;
        FontSize = size;
        FontName = font;
        Italic = italic;
        Bold = bold;
        Alignment = BaseLine + Left;
    }

    public Rectangle BoundingBox()
    {
        // TODO: Incorporate this into drawing size
        return Rectangle.Empty;
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.Write($"<text x=\"{location.X}\" y=\"{location.Y}\" style=\"font:");
        if (Italic)
            sw.Write(" italic");
        if (Bold)
            sw.Write(" bold");
        sw.Write($" {FontSize} {FontName};\"");
        if (!string.IsNullOrEmpty(Fill))
            sw.Write($" fill=\"{Fill}\"");
        sw.Write(AlignmentAttributes());
        sw.Write($">{WebUtility.HtmlEncode(text)}</text>");
        return sw.ToString();
    }
}
