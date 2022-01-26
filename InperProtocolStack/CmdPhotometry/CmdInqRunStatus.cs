using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInqRunStatus : Basis.Command
    {
        public CmdInqRunStatus()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_INQ_RUN_STATUS;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
