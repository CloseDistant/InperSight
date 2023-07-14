﻿using System;
using System.Collections.Generic;

namespace InperProtocolStack.CmdPhotometry
{
    public class CmdAppRun : Basis.Command
    {
        public CmdAppRun()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_CMD_APP_RUN;
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(0));

            _CmdDataFrame.FrameBody = b;
        }
    }
}