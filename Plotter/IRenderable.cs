using System.Collections.Generic;
using TwoDimensionLib;

namespace Plotter;

public enum LineCap
{
    None,
    Butt,
    Square,
    Round
}

public enum LineJoin
{
    None,
    Mitre,
    Round,
    Bevel
}

public interface IRenderable
{
    string Stroke { get; set; }

    string StrokeWidth { get; set; }

    string Fill { get; set; }

    LineCap Cap { get; set; }

    LineJoin Join { get; set; }

    IEnumerable<int> Dashes { get; set; }
    void SetDashes(params int[] dashes);

    Rectangle BoundingBox();
}
