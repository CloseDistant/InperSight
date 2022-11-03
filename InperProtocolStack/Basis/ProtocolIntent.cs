namespace InperProtocolStack.CmdPhotometry
{
    public class ProtocolIntent
    {
        public const byte INTENT_CFM = 0xFF;
        public const byte INTENT_HBT = 0xFE;

        ///Bootloader///
        public const byte INTENT_CMD_SETUP = 0xEF;
        public const byte INTENT_CMD_PROG = 0xEE;
        public const byte INTENT_CMD_CCRC = 0xED;
        public const byte INTENT_CMD_LOCK = 0xE0;

        ///Settings & inquiry Intent///
        public const byte INTENT_SAVE_ID = 0x00;
        public const byte INTENT_INQ_ID = 0x01;
        public const byte INTENT_RETURN_ID = 0x02;
        public const byte INTENT_SAVE_LIGHT_CONFIG = 0x03;
        public const byte INTENT_INQ_LIGHT_CONFIG = 0x04;
        public const byte INTENT_RETURN_LIGHT_CONFIG = 0x05;
        public const byte INTENT_SAVE_IO_CONFIG = 0x06;
        public const byte INTENT_INQ_IO_CONFIG = 0x07;
        public const byte INTENT_RETURN_IO_CONFIG = 0x08;

        ///Functional Intent///
        public const byte INTENT_START = 0x40;
        public const byte INTENT_STOP = 0x41;
        public const byte INTENT_SET_GBL_WF = 0x50;
        public const byte INTENT_SET_CHN_SWEEP = 0x51;
        public const byte INTENT_SET_SWEEP_STATE = 0x52;
        public const byte INTENT_MEASURE_MODE = 0x10;
        public const byte INTENT_LIGHT_HOLD_ON = 0x11;
        public const byte INTENT_INQ_INFO = 0x12;
        public const byte INTENT_RETURN_INFO = 0x13;
        public const byte INTENT_INQ_RUN_STATUS = 0x14;
        public const byte INTENT_RETURN_RUN_STATUS = 0x15;
        public const byte INTENT_SET_BIND_DIO = 0x16;
        public const byte INTENT_SET_EXPOURSE = 0x19;
        public const byte INTENT_SET_FRAME_RATE = 0x20;
        public const byte INTENT_SWITCH_LIGHT_MODE = 0x21;
        public const byte INTENT_SET_LIGHT_PWR = 0x22;
        public const byte INTENT_IO_STATUS = 0x23;
        public const byte INTENT_SET_TRIGGER = 0x24;
        public const byte INTENT_INPUT_IO = 0x30;
        public const byte INTENT_OUTPUT_IO = 0x31;
        public const byte INTENT_TRIGGER_CHANGE = 0x32;
        public const byte INTENT_ADFRAME_RATE = 0x33;

        public const byte INTENT_FILE_HASH_VAL = 0xEF;
        public const byte INTENT_FILE_UPLOAD = 0xE9;
        public const byte INTENT_CMD_GET_HASH_VAL = 0xEE;
        public const byte INTENT_CMD_INIT_MCU = 0xED;
        public const byte INTENT_CMD_SET_FILE_LENGTH = 0xEC;
        public const byte INTENT_FILE_LENGTH = 0xEB;
        public const byte INTENT_CMD_APP_RUN = 0xEA;

        public const byte INTENT_DEVIC_INFO = 0xE5;
        public const byte INTENT_PHOTOMETRY_INFO = 0xE7;
        public const byte INTENT_DEVICE_INFO_PUB = 0xE8;

    }
}
