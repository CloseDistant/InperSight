using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetLightPower : Basis.Command
    {
        public CmdSetLightPower()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_LIGHT_PWR;
        }


        public void SetCmdParam(UInt32 light_id, float power)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(light_id));
            b.AddRange(BitConverter.GetBytes(power));
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
