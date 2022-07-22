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
