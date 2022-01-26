using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInqID : Basis.Command
    {
        public CmdInqID()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_INQ_ID;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
