using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdSaveID : Basis.Command
    {
        public CmdSaveID()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_SAVE_ID;
        }


        public void SetCmdParam(UInt32 software_id, UInt32 hardware_id, string camera_id)
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(software_id));
            b.AddRange(BitConverter.GetBytes(hardware_id));
            b.AddRange(Encoding.ASCII.GetBytes(camera_id));
            _CmdDataFrame.FrameBody = b;

            return;
        }
    }
}
