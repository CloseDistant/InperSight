using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class AdditionRecordConditions
    {
        public bool Immediately { get; set; }
        public Delay Delay { get; set; }
        public AtTime AtTime { get; set; }
        public Trigger Trigger { get; set; }
    }
    public class Delay : AdditionBase
    {
        public double Value { get; set; }
    }
    public class AtTime : AdditionBase
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
    }

    public class Trigger : AdditionBase
    {
        public string Source { get; set; }
        public int PriTrigger { get; set; }
        public double ThreshodMin { get; set; }
        public double ThreshodMax { get; set; }
    }

    public abstract class AdditionBase
    {
        public bool IsActive { get; set; }
    }
}
