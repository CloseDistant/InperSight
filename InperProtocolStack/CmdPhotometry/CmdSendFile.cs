using System;
using System.Collections.Generic;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSendFile : Basis.Command
    {
        public CmdSendFile()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_FILE_UPLOAD;
        }
        public void SetCmdParam(List<byte> bytes, uint addr = 0)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(addr));
            b.AddRange(bytes);

            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
