namespace InperProtocolStack.CmdPhotometry
{
    public class CmdInqInfo : Basis.Command
    {
        public CmdInqInfo()
        {
            _CmdDataFrame.Header.Intent = ProtocolIntent.INTENT_INQ_INFO;
        }


        public void SetCmdParam()
        {
            return;
        }
    }
}
