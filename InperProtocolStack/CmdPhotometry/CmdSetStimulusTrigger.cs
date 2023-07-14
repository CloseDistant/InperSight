using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetStimulusTrigger : Basis.Command
    {
        public CmdSetStimulusTrigger()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_SWEEP_TRIGGER;
        }
        public void SetCmdParam(byte state, int ioid, byte mode)
        {
            List<byte> b = new List<byte>();
            string ob = "00000000";
            ob = ob.Insert(7 - ioid, "1").Remove(8, 1);
            b.Add(state);
            b.Add(System.Convert.ToByte(ob, 2));
            //b.AddRange(BitConverter.GetBytes(ioid));
            b.Add(mode);
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
