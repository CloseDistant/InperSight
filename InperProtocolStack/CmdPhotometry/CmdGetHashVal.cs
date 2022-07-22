using System;
using System.Collections.Generic;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdGetHashVal : Basis.Command
    {
        public CmdGetHashVal()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_CMD_GET_HASH_VAL;
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(0));

            _CmdDataFrame.FrameBody = b;
        }
    }
}
