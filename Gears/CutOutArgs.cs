using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Arguments describing the spindle holes in the middle of a sprocket")]
    internal class CutOutArgs
    {
        [Required]
        [Arg("--spindle")]
        [Arg("-s")]
        [Description("Diameter of hole through which the axle/spindle is inserted")]
        public double SpindleDiameter { get; set; }

        [Arg("--inlay")]
        [Arg("-i")]
        [Description("Diameter of an inlay around the spindle, for inserting a ball-race")]
        public double InlayDiameter { get; set; } = 0;

        [Arg("--keyflats")]
        [Arg("-k")]
        [Description("Distance across flats of a hexagonal inlay if locking adjacent sprockets together")]
        public double KeyFlatWidth { get; set; } = 0;
    }
}
