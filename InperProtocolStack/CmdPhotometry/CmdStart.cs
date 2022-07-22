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
