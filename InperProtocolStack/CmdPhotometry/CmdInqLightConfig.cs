using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInqLightConfig : Basis.Command
    {
        public CmdInqLightConfig()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_INQ_LIGHT_CONFIG;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
