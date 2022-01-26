using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdOutputIO : Basis.Command
    {
        public CmdOutputIO()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_OUTPUT_IO;
        }


        public void SetCmdParam(UInt32 io_id, UInt32 stat)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(io_id));
            b.AddRange(BitConverter.GetBytes(stat));
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
