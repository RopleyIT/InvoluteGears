using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdArgs;
namespace Gears
{
    [ArgSet]
    [Description("Arguments specific to the design of involute gears")]
    internal class InvoluteArgs
    {
        [Arg("--profileshift")]
        [Arg("-ps")]
        [Description("The radial profile shift of the gear teeth as % of module")]
        public double ProfileShift { get; set; } = 0;

        [Required]
        [Arg("--pressureangle")]
        [Arg("-pa")]
        [Description("The angle relative to the line between gear "
            + "centres along which force is applied between teeth")]
        public double PressureAngle { get; set; }
    }
}
