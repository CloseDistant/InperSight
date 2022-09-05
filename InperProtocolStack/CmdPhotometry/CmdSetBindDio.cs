using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetBindDio : Basis.Command
    {
        public CmdSetBindDio()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_BIND_DIO;
        }
        public void SetCmdParam(List<byte> bytes)
        {
            _CmdDataFrame.FrameBody = bytes;
            return;
        }
    }
}
