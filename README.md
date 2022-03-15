## Involute Gear Calculator
This application is used to calculate designs for various types of gears, 
and to render their outlines. It can be used to prepare SVG files for 
export to CAD tools or CNC machines, or can just save images of gears as JPEG files. 
The gear calculator is a command line/shell script application, that uses the 
command line for input, and either the standard output or a text file for 
computed results. 

The application is also made available as a small web application, implemented using
Blazor web interface technology. Everything that can be done from the command line
is available, and more easily accessed, from the web app when launched.

The command line application is invoked as follows:

`gears [primary-option] [option-specific-arguments]`

Every command has one primary option, followed by multiple additional arguments. 
The arguments that follow the primary option depend in type and number 
on which primary option has been selected.

#### Primary options

The following table describes what each primary option will do for you

| Option | Description |
| ------ | ----------- |
| `involute` | Creates an involute gear profile based on the additional arguments |
| `cycloid` | Creates a gear based on epi- and hypocycloids as used in clock movements |
| `ratchet` | Creates a gear that can be used with a pawl to make a one-way ratchet |
| `roller` | Sprockets for roller chains such as bicycle chains are created from this option |
| `escape` | A wheel designed to be used in a pendulum escapement |
| `chain` | A wheel designed to carry a conventional clock chain |
| `-m` or `--matchedratios` | Finds pinion sprocket pairs that have the same modulus sums that give a particular gear ratio |
| `-C` or `--contactratios` | Saves to a file a table of involute gear contact ratios for various pressure angles and tooth counts |
| `-c` or `--customratios` | As for the `-C` option, but allows custom selection of pressure angles and tooth counts |


#### Gear generation common options

Regardless of which primary option has been selected, there are a number of follow-on options that are applied
to many different gear types being generated.

Each option has a short form with a hyphen and a single letter, or a longer form
beginning with two hyphens. For example, `--profileshift 6` and `-ps 6` perform the same task, setting
the desired profile shift of the gear to 6%. What follows is a chart of the common options to be set for
various types of output gear:

| Option | Description |
| -- | -- |
| `-t` or `--teeth` | The number of teeth around the perimeter of this gear |
| `-a` or `--accuracy` | The accuracy with which the paths are plotted. Lower accuracy allows fewer points |
| `-b` or `--backlash` | Allows slack between teeth and between gears to reduce friction. Measured as a fraction of the module |
| `-m` or `--module` | Millimetres of diameter per tooth. Pitch circle diameter = module * number of teeth |
| `-c` or `--cutter` | The diameter of the end mill used to cut out the teeth. Gives a minimum curvature for shapes |

#### Cut out options

In the middle of a gear it is possbile to cut out spokes, a central spindle hole for the axle to be inserted,
and even an inlay for a ball bearing and/or a keyed cutout to lock adjacent teeth together. The options
to configure these can be set for all gear types. The application will use its own algorithm to decide
whether the gear is big enough to warrant creating spokes, but the other spindle options are all
available to you to set:

| Option | Description |
| -- | -- |
| `-s` or `--spindle` | Diameter of hole cut through centre of gear |
| `-i` or `--inlay` | Diameter of a second circle in the middle of the gear for an inlaid bearing |
| `-k` or `--keyflats` | The distance across the flats of a hexagon in the middle of the gear for locking two adjacent gears to each other |

#### Involute gears

In addition to the common and cut out options described above, each gear type has specific options that
apply only to gears of that type. For involute gears, these options are listed below.

| Option | Description |
| -- | -- |
| `-ps` or `--profileshift` | Defaulting to zero, this allows you to set a positive or negative profile shift as a perentage of the module |
| `-pa` or `--pressureangle` | The contact angle between adjacent teeth in the involute gear. COmmonly 14.5 or 20 degrees |

An example of a command line for building an involute gear might be as follows:

`gears involute -t 18 -a 0.01 -m 5 -c 3.18 -s 6 -pa 14.5`

This creates two files - a JPEG file and an SVG file for the same gear. The gear has 18 teeth, with
a module of 5mm. It is designed to be cut out with a 3.18mm diameter end mill or smaller. It has a
spindle hole diameter of 6mm and the teeth always make contact at 14.5 degrees to the tangent passing
through the contact point. Finally by setting the plotting accuracy to the nearest 0.01 mm, the number
of points in the plot is kept low.

Default values are used for the missing parameters. There is zero backlash (unwise, as machining errors
may lead to friction), zero profile shift where the gears are moved closer together or further apart and
the tooth shapes adjusted accordingly, and no bearing inlay or hexagonal keying.

A unique filename for each of the output files is created, based on the parameter values passed on the
command line.

#### Clock chain sprockets

The set of arguments speialised for a clock chain are given below. Note that when we use the term 'clock
chain', we mean a chain made of circular wire loops folded into rings that are interleaved with each
other. In other words, a classic chain as might be used for a barrier or indeed to hang clock weights.

| Option | Description |
| -- | -- |
| `-wt` or `--wirethckness` | The thickness of the wire used to make the chain links |
| `-li` or `--linkength` | The length of each link between inside edges of the chain link |
| `-wo` or `--linkwidth` | The external width between outer edges of the each link |

#### Cycloid gears

In clock making, involute gears have historically not been used very much. Traditionally gears based
on epicycloids and their mating hypocycloids would have been used. These have the advantage that
machining the hypocycloids can be made extremely easy, but the disadvantage is that the gears are
very sensitive to separation distance between the gear centres. One valuable benefit is that gears
with very small numbers of teeth can be designed without those gears binding or being weakened by
undercutting. This is useful for high gearing ratios.

| Option | Description |
| -- | -- |
| `-on` or `--opposingteeth` | The number of teeth on the gear that will be meshing with the one you are desigining |
| `-tb` or `--blunting` | The percentage of the full height of the tooth that is 'sanded off' to reduce the tooth height |
| `-ob` or `--opposingblunting` | The percentage of the opposite tooth's height that has been removed |

Some notes on these options might be helpful. The full height of the cycloid tooth is the distance radially
from the pitch circle out to the point at which the rising epicycloid crosses the falling epicycloid for the
back of the tooth, i.e. the middle of the tooth. We blunt the tooth so that the dedendum of the tooth
opposite is reduced, meaning we don't have long thin teeth that could be quite weak. The downside is that
the contact ratio (the average number of teeth in contact between the two gears) reduces. We never want
the contact ratio to drop below unity as that would mean there are times in which no two teeth are in contact.

There is another benefit too. Involute gears have a constant pressure angle between the teeth throughout their
contact time. Cycloidal gears have a pressure angle that starts quite severe near the ends of the teeth
but passes through zero when the contact point lies on a line between the two gear centres. By blunting
the teeth, we are able to remove some of the contact time that occurs at severe angles thus reducing the 
maximum pressure angle.

#### Escape wheels

Clock movements have special wheels that often have sharp angular teeth used to drive the pallets
attached to the top of a pendulum. As the pendulum mechanism is known as the escapement, these wheels
are known as espace wheels.

Escape wheels are characterised by having a flat face on the leading edge of each tooth, a tooth tip
width for strength, and a sloping rear to each tooth to maintain the tooth strength. Between each tooth
at its dedendum there is an arc that joins the back of one tooth the the face of the next tooth. The
effect is to create a tooth that looks like a circular saw blade. To see some examples of this, try
experimenting with the options in the table below:

| Option | Description |
| -- | -- |
| `-ua` or `--undercut` | The angle of slope backwards of the facing edge of teeth relative to a radial |
| `-fl` or `--facelength` | The length of the flat facing edge of each tooth, at the slope angle |
| `-tp` or `--tippitch` | The circumferential length of the 'flat' tip of each tooth |
| `-bd` or `--basediameter` | The diameter of the arc joining the base of one tooth to the back of the next |

#### Ratchet wheels

A ratchet when combined with a ratchet pawl allows a wheel to turn in one direction but prevents
rotation in the opposite direction. These are often used for winding mechanisms such as lifting weights.
Ratchets are actually similar in design to escape wheels, except there is a gradually rising curve from
the base of one tooth to the tip of the next. The drop from a tooth tip to the base should be abrupt so
that there is no chance of a pawl bouncing up and allowing the wheel to slip backwards.

| Option | Description |
| -- | -- |
| `-id` or `--innerdiameter` | The outer diameter of the ratchet is set by the module and tooth count. The inner diameter value therefore allow us to set the depth of the ratchet teeth |

#### Roller chain sprockets

Sprockets for clock chains are much less common that the sprockets we are all familiar with for roller chains.
An obvious example of this is the classic bike chain with its characteristic bike gear sprockets. This
gear type is used when designing a sprocket for a roller chain of this type.

| Option | Description |
| -- | -- |
| `-rp` or `--pitch` | The distance between rollers in the roller chain |
| `-rd` or `--rollerdiameter` | The diameter of the cylindrical rollers in the chain |
| `-cw` or `--chainwidth` | The breadth of the side plates to each side of the rollers. This is NOT the width of the rollers |

The reason we need the chain width is because the designed sprocket can allow the edges of the chain to
lie on a circular base to either side if the sprocket teeth if desired.

#### Option -m

`gears -m [numerator] [denominator] [min-teeth] [max-teeth]`

Sometimes we need to map a gear ratio back onto the same axle using two gear pairs. For example,
this happens in a clock movement where the hour hand must rotate at one twelfth
the rate of the minute hand, but is coaxial with it. Two gears are used to achieve 
the 1/12th reduction, using the two gear ratios 1/4 and 1/3 for example. This option
allows you to find the number of teeth on both the gear and the pinion for the two gears
so that they have the same axle separations, for any overall gear ratio of numerator/denominator.

| Argument | Description |
| -------- | ----------- |
| `numerator` | The numerator of the numerator and denominator pair that define the overall gear ratio to be achieved by the two gears. |
| `denominator` | The denominator. The ratio of gearing will be numerator/denominator. |
| `min-teeth` | The minimum number of teeth you would tolerate on a pinion when searching for gears. |
| `max-teeth` | The maximum number of teeth, at which the search ceases. |

Example: `gears -m 1 60 10 96`

This would search for all combinations of pinion and gear tooth counts for pairs of gears
that together have the same axle separation, but that also give an overall ratio of
1/60th. No tooth or pinion will have fewer than 10 teeth, nor more than 92 teeth.

#### Option -C

`gears -C [output-file-path]`

Creates a table of data for a range of gears 
with various pressure angles, tooth counts,
and profile shifts. The results are written to the file given 
in the output file path. For each tooth it tabulates the following data:

| Item | Description |
| ---- | ----------- |
| GAP | The linear distance between the two undercut points on adjacent teeth of the gear. Used to tell you the maximum diameter of the end-mill cutter to use. |
| Db | The base circle diameter from which the involute tooth curve starts. |
| Dd | The dedendum circle diameter. This is the innermost depth the tooth is cut to, if we were able to use a small end-mill. |
| Dc | The reduced dedendum diameter caused by using a cutter of diameter too great to follow the ideal undercut curve. |
| Du | The diameter at which the undercut begins to eat into the involute curve. |
| Dp | The pitch circle diameter for the tooth. This is usually `module * tooth-count`. |
| Da | The addendum diameter of the gear. This is the outermost diameter and includes any extra diameter introduced by profile shifting. |

Following this list, a table appears that shows 
for each pair of gears cut to this profile-shift and pressure angle,
what the contact ratio is between two gears with 
their respective numbers of teeth. All combinations
of tooth pairs from the range 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 are plotted.

The contact ratio is defined as the 
average number of teeth that are in contact on the
involute part of their gear surfaces at 
any one time. If less than 1, the gear tips will be grinding
on the undercut parts for some of their 
rotation, and will run unevenly and noisily.
Ideally this ratio should be above about 1.1 
for smooth handover between teeth.

#### Option -c

`gears -c [pressure-angle] [teeth1,teeth2 ... teethM] [module] [profile-shift1] [profile-shift2] [cutter-diameter] [output-filename]`

Creates a table of data for the gears whose 
pressure angle and comma-separated list of tooth counts have been specified
in the arguments. The comma-separated arguments must not 
have any embedded spaces.

Table data is as for the `-C` option
described above, but with a single selected pressure 
angle, a seleted list of tooth counts,  a module, and cutter diameter. 
Two profile shifts are provided so that the contact ratios between
pairs of gears each with a different profile shift can be listed. The pressure
angle is specified in tenths of a degree, the profile shifts are percentages of the module,
and the module and cutter diameter in 100ths of a mm.

For the `-C` option, we assume a sufficiently small
diameter cutter so that the undercut and dedendum do not need 
to be adjusted from the ideal curve. Also for the
`-C` option, we assume the module is 1.0mm as a working default.

Example: `gears -c geardata.txt 145 10,12,15,45,48,50 500 21 -21 635 output.txt`

Computes the tables for the 14.5 degree pressure angle, for all gear tooth counts in the
list of values supplied as the second parameter. The gears have a module of 5mm, and
are cut using an end mill of diameter 1/4 inch (6.35mm). Given a profile shift of 21%
and -21%, all contact ratios between every pair of gears specified in 
the list of tooth counts for profile shifts either way round are listed. The results are
written to the file `output.txt`.

### Licensing

This product is published under the [standard MIT License](https://opensource.org/licenses/MIT). The specific wording for this license is as follows:

Copyright 2020 [Ropley Information Technology Ltd.](http://www.ropley.com)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

