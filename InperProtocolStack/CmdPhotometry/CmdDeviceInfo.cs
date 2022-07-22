using System;
using System.Collections.Generic;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdDeviceInfo : Basis.Command
    {
        public CmdDeviceInfo()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_DEVIC_INFO;
        }
        public void SetCmdParam(uint body)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(body));
            _CmdDataFrame.FrameBody = b;
        }
    }
}
