using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdArgs;
namespace Gears
{
    [ArgSet]
    [Description("Arguments specific to the design of roller chain sprockets")]
    internal class RollerSprocketArgs
    {
        [Required]
        [Arg("--pitch")]
        [Arg("-rp")]
        [Description("The distance between adjacent chain links")]
        public double Pitch { get; set; }

        [Required]
        [Arg("--rollerdiameter")]
        [Arg("-rd")]
        [Description("The diameter of each chain roller")]
        public double RollerDiameter { get; set; }

        [Required]
        [Arg("--chainwidth")]
        [Arg("-cw")]
        [Description("The max width of the chain side plates")]
        public double ChainWidth { get; set; }
    }
}
