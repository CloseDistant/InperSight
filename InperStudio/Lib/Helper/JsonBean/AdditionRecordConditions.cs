using System.Collections.Generic;

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
        public string Mode { get; set; }
        public int PriTrigger { get; set; }
        public int RefractoryPeriod { get; set; }
        public int Duration { get; set; }
        public int DioId { get; set; }
        public string Name { get; set; }
        public double ThreshodMin { get; set; }
        public double ThreshodMax { get; set; }
    }

    public abstract class AdditionBase
    {
        public bool IsActive { get; set; }
    }
}
