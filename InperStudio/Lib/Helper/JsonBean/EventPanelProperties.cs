using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class EventPanelProperties
    {
        public bool HeightAuto { get; set; } = true;
        public bool HeightFixed { get; set; } = false;
        public double HeightFixedValue { get; set; } = 100;
        public bool DisplayLockedBottom { get; set; } = true;
        public bool DisplayNameVisible { get; set; } = true;
    }
}
