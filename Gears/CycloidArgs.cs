using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Arguments needed for epi/hypocycloidal gears, which are always paired")]
    internal class CycloidArgs
    {
        [Required]
        [Arg("--opposingteeth")]
        [Arg("-on")]
        [Description("Number of teeth on gear with which our gear will be meshing")]
        public int OpposingTeeth { get; set; }

        [Arg("--blunting")]
        [Arg("-tb")]
        [Description("Percentage of maximum tooth length before the end is chopped short")]
        public double ToothBlunting { get; set; } = 0;

        [Arg("--opposingblunting")]
        [Arg("-ob")]
        [Description("Tooth blunting for wheel that meshes with the wheel we are designing")]
        public double OpposingToothBlunting { get; set; } = 0;
    }
}
