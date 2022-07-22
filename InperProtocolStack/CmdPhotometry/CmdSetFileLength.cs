using System;
using System.Collections.Generic;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetFileLength : Basis.Command
    {
        public CmdSetFileLength()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_CMD_SET_FILE_LENGTH;
        }
        public void SetCmdParam(UInt32 length, UInt32 addr = 0)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(addr));
            b.AddRange(BitConverter.GetBytes(length));
            _CmdDataFrame.FrameBody = b;
        }
    }
}
