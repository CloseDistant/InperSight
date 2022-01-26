using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetFrameRate : Basis.Command
    {
        public CmdSetFrameRate()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_FRAME_RATE;
        }

        
        public void SetCmdParam(float frame_rate)
        {
            _CmdDataFrame.FrameBody = BitConverter.GetBytes(frame_rate).ToList();
            return;
        }
    }
}
