using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.Basis
{
    public class Command
    {
        protected DataFrame _CmdDataFrame = new DataFrame(new FrameHeader(), new List<byte>());


        public UInt32 Sequence
        {
            get
            {
                return _CmdDataFrame.Header.Sequence;
            }
            set
            {
                _CmdDataFrame.Header.Sequence = value;
            }
        }


        public UInt32 AckNumber
        {
            get
            {
                return _CmdDataFrame.Header.AckNumber;
            }
            set
            {
                _CmdDataFrame.Header.AckNumber = value;
            }
        }


        public byte Intent => _CmdDataFrame.Header.Intent;


        public List<byte> GetBytes()
        {
            return _CmdDataFrame.GetBytes();
        }
    }
}
