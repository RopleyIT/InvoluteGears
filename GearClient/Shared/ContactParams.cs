using System;
using System.Collections.Generic;
using System.Text;

namespace GearClient.Shared
{
    public class ContactParams
    {
        public IList<int> PressureAngles { get; set; }
        public IList<int> Teeth { get; set; }
        public int Module { get; set; }
        public int CutterDiameter { get; set; }
    }
}
