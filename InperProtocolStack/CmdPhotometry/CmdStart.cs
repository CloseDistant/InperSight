using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdStart : Basis.Command
    {
        public CmdStart()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_START;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
