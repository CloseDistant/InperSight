using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInitMcu : Basis.Command
    {
        public CmdInitMcu()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_CMD_INIT_MCU; 
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(0));
            _CmdDataFrame.FrameBody = b;
        }
       
    }
}
