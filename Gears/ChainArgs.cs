using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Arguments specific to conventional link chain sprockets")]
    internal class ChainArgs
    {
        [Required]
        [Arg("--wirethickness")]
        [Arg("-wt")]
        [Description("Sets the diameter of the wire from which the chain was made")]
        public double WireThickness { get; set; }

        [Required]
        [Arg("--linklength")]
        [Arg("-li")]
        [Description("The length of each link between inside edges of the chain link")]
        public double InnerLinkLength { get; set; }

        [Required]
        [Arg("--linkwidth")]
        [Arg("-wo")]
        [Description("The external width between outer edges of the each link")]
        public double OuterLinkWidth { get; set; }
    }
}
