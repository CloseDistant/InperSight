using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSendExposure : Basis.Command
    {
        public CmdSendExposure()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_EXPOURSE;
        }
        public void SetCmdParam(uint addr = 0)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(addr));

            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
