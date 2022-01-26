using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetIOStatus : Basis.Command
    {
        public CmdSetIOStatus()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_IO_STATUS;
        }


        public void SetCmdParam(UInt32 io_id, UInt32 status)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(io_id));
            b.AddRange(BitConverter.GetBytes(status));
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
