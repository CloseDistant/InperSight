using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInqIOConfig : Basis.Command
    {
        public CmdInqIOConfig()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_INQ_IO_CONFIG;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
