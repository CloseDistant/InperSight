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
