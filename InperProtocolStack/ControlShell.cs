﻿using InperProtocolStack.Basis;
using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.Communication;
using InperProtocolStack.TransmissionCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace InperProtocolStack
{
    public class LightDesc
    {
        public UInt32 LightID { get; private set; }
        public UInt32 WaveLength { get; private set; }
        public float MaxPowerInW { get; private set; }

        public LightDesc(UInt32 id, UInt32 wave_length, float max_power_w)
        {
            LightID = id;
            WaveLength = wave_length;
            MaxPowerInW = max_power_w;
        }
    }


    public class DevInfoUpdatedEventArgs : EventArgs
    {

    }


    public class DevNotificationEventArgs : EventArgs
    {

    }


    public class DevInputNotificationEventArgs : DevNotificationEventArgs
    {
        public UInt32 IOID;
        public UInt32 Index;
        public UInt32 Status;
        public UInt64 Timestamp;
    }


    public class ControlShell
    {
        private UARTAgent _UARTA;
        private TransTrafficCtrl _TC;
        public ControlShell(string p_name)
        {
            _UARTA = new UARTAgent(p_name);
            _TC = new TransTrafficCtrl(_UARTA);
            _TC.OnInfoUpdated += InfoUpdated;
            InqID();
        }


        public uint FirmwareRevision { get; private set; } = 0;
        public uint HardwareRevision { get; private set; } = 0;
        public string BundleCameraID = "";

        #region Update Device Infomation
        public event EventHandler<DevInfoUpdatedEventArgs> OnDevInfoUpdated;
        //public event EventHandler<DevInfoUpdatedEventArgs> OnUsbDevInfoUpdated;

        private readonly int _FwRevisionLen = 4;
        private readonly int _HwRevisionLen = 4;
        private readonly int _CameraIDLen = 16;
        private void UpdateID(byte[] param)
        {
            if (param.Length < (_FwRevisionLen + _HwRevisionLen + _CameraIDLen))
            {
                return;
            }

            byte[] fr_bs = param.Take(_FwRevisionLen).ToArray();
            byte[] hr_bs = param.Skip(_FwRevisionLen).Take(_HwRevisionLen).ToArray();
            byte[] cid_bs = param.Skip(_FwRevisionLen + _HwRevisionLen).Take(_CameraIDLen).ToArray();

            FirmwareRevision = BitConverter.ToUInt32(fr_bs, 0);
            HardwareRevision = BitConverter.ToUInt32(hr_bs, 0);
            BundleCameraID = Encoding.ASCII.GetString(cid_bs);

            return;
        }


        private struct LightConfig
        {
            public UInt32 ID;
            public UInt32 Enabled;
            public float MaxPower;
            public UInt32 WaveLength;
        }
        public List<LightDesc> LightSourceList { get; private set; } = new List<LightDesc>();
        private void UpdateLightConfig(byte[] param)
        {
            int light_cfg_size = Marshal.SizeOf(typeof(LightConfig));
            UInt32 light_no = BitConverter.ToUInt32(param.Take(4).ToArray(), 0);
            for (int li = 0; li < light_no; li++)
            {
                byte[] cfg_bytes = param.Skip(4 + (li * light_cfg_size)).Take(light_cfg_size).ToArray();
                LightConfig lc = Utils.BytesToStruct<LightConfig>(cfg_bytes, light_cfg_size);
                if (lc.Enabled != 0)
                {
                    LightDesc ld = new LightDesc(lc.ID, lc.WaveLength, lc.MaxPower);
                    LightSourceList.Add(ld);
                }
            }
            return;
        }

        private void UpdateIOConfig(byte[] param)
        {
            return;
        }
        #endregion


        #region Device Notification
        public event EventHandler<DevNotificationEventArgs> OnDevNotification;
        private DevInputNotificationEventArgs DeviceInputUpdated(byte[] param)
        {
            if (param.Length < 8)
            {
                return null;
            }

            UInt32 io_id = BitConverter.ToUInt32(param.Take(4).ToArray(), 0);
            byte[] stat_b = param.Skip(4).Take(4).ToArray();
            UInt32 io_stat = BitConverter.ToUInt32(stat_b, 0);

            DevInputNotificationEventArgs e = new DevInputNotificationEventArgs { IOID = io_id, Status = io_stat };
            return e;
        }
        #endregion


        private void InfoUpdated(object sender, MessageReceivedEventArgs e)
        {
            switch (e.Intent)
            {
                case ProtocolIntent.INTENT_RETURN_ID:
                    UpdateID(e.Data);
                    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                    break;

                case ProtocolIntent.INTENT_RETURN_LIGHT_CONFIG:
                    UpdateLightConfig(e.Data);
                    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                    break;

                case ProtocolIntent.INTENT_RETURN_IO_CONFIG:
                    UpdateIOConfig(e.Data);
                    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                    break;

                case ProtocolIntent.INTENT_INPUT_IO:
                    DevInputNotificationEventArgs ea = DeviceInputUpdated(e.Data);
                    OnDevNotification?.Invoke(this, ea);
                    break;

                default:
                    break;
            }


        }


        public void SetFrameRate(double frame_rate)
        {
            CmdSetFrameRate cmd = new CmdSetFrameRate();
            cmd.SetCmdParam((float)frame_rate);
            _TC.Transmit(cmd);

            return;
        }


        public void SwitchLight(uint light_id, bool enable)
        {
            CmdSwitchLight cmd = new CmdSwitchLight();
            cmd.SetCmdParam(light_id, Convert.ToUInt32(enable));
            _TC.Transmit(cmd);

            return;
        }


        public void SetLightPower(uint light_id, double power)
        {
            CmdSetLightPower cmd = new CmdSetLightPower();
            cmd.SetCmdParam(light_id, (float)power);
            _TC.Transmit(cmd);

            return;
        }


        public void SaveID(uint software_id, uint hardware_id, string camera_id)
        {
            CmdSaveID cmd = new CmdSaveID();
            cmd.SetCmdParam(software_id, hardware_id, camera_id);
            _TC.Transmit(cmd);
        }


        public void InqID()
        {
            CmdInqID cmd = new CmdInqID();
            _TC.Transmit(cmd);
        }


        public void InqLightConfig()
        {
            CmdInqLightConfig cmd = new CmdInqLightConfig();
            _TC.Transmit(cmd);
        }


        public void SetMeasureMode(bool enable)
        {
            CmdMeasureMode cmd = new CmdMeasureMode();
            cmd.SetCmdParam(enable);
            _TC.Transmit(cmd);
        }


        public void SetIOMode(uint io_id, uint io_mode)
        {
            CmdSetIOStatus cmd = new CmdSetIOStatus();
            cmd.SetCmdParam(io_id, io_mode);
            _TC.Transmit(cmd);
        }


        public void OuputIO(uint io_id, uint stat)
        {
            CmdOutputIO cmd = new CmdOutputIO();
            cmd.SetCmdParam(io_id, stat);
            _TC.Transmit(cmd);
        }

        public void Start()
        {
            CmdStart cmd = new CmdStart();
            _TC.Transmit(cmd);
        }

        public void Stop()
        {
            CmdStop cmd = new CmdStop();
            _TC.Transmit(cmd);
        }
    }
}
