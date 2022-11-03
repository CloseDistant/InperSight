using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSetSweepState : Basis.Command
    {
        public CmdSetSweepState()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_SWEEP_STATE;
        }
        public void SetCmdParam(int state)
        {
            _CmdDataFrame.FrameBody = BitConverter.GetBytes(state).ToList();

            return;
        }
    }
}
