using InperProtocolStack.Basis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public struct CHNSweepStruct
    {
        public int DioID;
        public float TotalTime;
        public SweepStruct[] SweepStructs;
    }
    public struct SweepStruct
    {
        public float Duration;
        public int BasicWaveformCount;
        public int[] WaveformID;
    }
    public class CmdSetCHNSweep : Command
    {
        public CmdSetCHNSweep()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_CHN_SWEEP;
        }
        public void SetCmdParam(List<byte> datas)
        {

            _CmdDataFrame.FrameBody = datas;// Utils.StructToBytes(cHNSweepStructs);

            return;
        }
    }
}
