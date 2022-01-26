using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSwitchLight : Basis.Command
    {
        public CmdSwitchLight()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SWITCH_LIGHT_MODE;
        }


        public void SetCmdParam(UInt32 light_id, UInt32 on_off)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(light_id));
            b.AddRange(BitConverter.GetBytes(on_off));
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
