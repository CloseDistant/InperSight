using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdStop : Basis.Command
    {
        public CmdStop()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_STOP;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
