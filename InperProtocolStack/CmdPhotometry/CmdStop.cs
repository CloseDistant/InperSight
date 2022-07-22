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
