using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InperStudio.Lib.Bean.Channel
{
    public class EventChannel : ChannelBase
    {
        private bool isActive = false;
        public bool IsActive { get => isActive; set => SetAndNotify(ref isActive, value); }
        public string BgColor { get; set; } = Brushes.Red.ToString();
        public string Hotkeys { get; set; }
        public bool DeltaF { get; set; }
    }
}
