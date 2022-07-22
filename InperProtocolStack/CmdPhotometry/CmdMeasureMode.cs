using System;
using System.Linq;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdMeasureMode : Basis.Command
    {
        public CmdMeasureMode()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_MEASURE_MODE;
        }


        public void SetCmdParam(bool enabled)
        {
            UInt32 cp = (UInt32)(enabled ? 1 : 0);
            _CmdDataFrame.FrameBody = BitConverter.GetBytes(cp).ToList();
            return;
        }
    }
}
