using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Common arguments shared by all (most) gear types")]
    internal class CommonArgs
    {
        [Required]
        [Arg("--teeth")]
        [Arg("-t")]
        [Description("The number of teeth around the edge of the sprocket")]
        public int Teeth { get; set; }

        [Arg("--accuracy")]
        [Arg("-a")]
        [Description("The accuracy of the points used to plot the sprockets in mm.")]
        public double Tolerance { get; set; } = 0;

        [Arg("--backlash")]
        [Arg("-b")]
        [Description("The amount of backlash to introduce, in mm.")]
        public double Backlash { get; set; } = 0;

        [Arg("--module")]
        [Arg("-m")]
        [Description("The module of the gear. Module is diameter of "
            + "sprocket at pitch circle divided by the number of teeth.")]
        public double Module { get; set; } = 1;

        [Arg("--cutter")]
        [Arg("-c")]
        [Description("The diameter of the end mill used to cut out the sprocket shape")]
        public double CutterDiameter { get; set; } = 0;
    }
}
