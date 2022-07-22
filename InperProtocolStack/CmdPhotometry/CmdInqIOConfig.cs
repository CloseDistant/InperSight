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
