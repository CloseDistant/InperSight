using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdAdframeRate : Basis.Command
    {
        public CmdAdframeRate()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_ADFRAME_RATE;
        }
        public void SetCmdParam(uint frameRate, uint[] array)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(frameRate));
            for (int i = 0; i < array.Length; i++)
            {
                b.AddRange(BitConverter.GetBytes(array[i]));
            }

            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
