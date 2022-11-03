using InperProtocolStack.Basis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public struct WaverformStruct
    {
        public int ID;
        public int WaveformType;
        public float PulseWidth;
        public float Frequency;
        public float PowerRegionLow;
        public float PowerRegionHigh;
        public float Duration;
        public float EC_A;
        public float EC_B;
    }
    public class CmdSetGBLWF : Basis.Command
    {
        public CmdSetGBLWF()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SET_GBL_WF;
        }
        public void SetCmdParam(WaverformStruct waverforms)
        {

            List<byte> datas = new List<byte>();

            Utils.StructToBytes(waverforms).ForEach(s => { datas.Add(s); });

            _CmdDataFrame.FrameBody = datas;

            return;
        }
    }
}
