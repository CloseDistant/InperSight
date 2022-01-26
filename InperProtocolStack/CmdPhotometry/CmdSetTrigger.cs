using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetTrigger : Basis.Command
    {
        public CmdSetTrigger()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_TRIGGER;
        }


        public void SetCmdParam(UInt32 trigger_id, UInt32 trigger_mode, float pre_trigger, float refractory_period, float threshold_low, float threshold_high)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(trigger_id));
            b.AddRange(BitConverter.GetBytes(trigger_mode));
            b.AddRange(BitConverter.GetBytes(pre_trigger));
            b.AddRange(BitConverter.GetBytes(refractory_period));
            b.AddRange(BitConverter.GetBytes(threshold_low));
            b.AddRange(BitConverter.GetBytes(threshold_high));

            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
