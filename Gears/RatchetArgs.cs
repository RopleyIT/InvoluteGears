using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Arguments specific to ratchet wheels")]
    internal class RatchetArgs
    {
        [Required]
        [Arg("--innerdiameter")]
        [Arg("-id")]
        [Description("The notional inner diameter of the ratchet, determining tooth depth")]
        public double InnerDiameter { get; set; }
    }
}
