using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.Communication;
using InperProtocolStack.TransmissionCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InperProtocolStack
{
    public class PhotometryInfo
    {
        public uint Model { get; set; }
        public uint DetectorIdLength { get; set; }
        public string DetectorId { get; set; }
        public uint SNlength { get; set; }
        public string SN { get; set; }
        public uint LightNumber { get; set; }
        public bool IsOutput { get; set; }
        public byte OutputId { get; set; }
        public List<LightConfigInfo> LightConfigInfos { get; set; }
    }
    public struct LightConfigInfo
    {
        public int ID;
        public float MaxPower;
        public uint WaveLength;
    }
    public class UsbControlShell
    {
        public readonly UARTAgent _UARTA;
        private TransTrafficCtrl _TC;
        public UsbControlShell(int vid, int pid)
        {
            _UARTA = new UARTAgent(vid, pid);
            _TC = new TransTrafficCtrl(_UARTA);
            _TC.OnInfoUpdated += InfoUpdated;
            _TC.OnInputUpdated += _TC_OnInputUpdated;
            InqID();
        }

        private void _TC_OnInputUpdated(object sender, byte[] e)
        {
            List<DevInputNotificationEventArgs> items = DeviceInputUpdated(e);
            if (items != null && items.Count > 0)
            {
                items.ForEach(x =>
                {
                    OnDevNotification?.Invoke(this, x);
                });
            }
        }

        public uint FirmwareRevision { get; private set; } = 0;
        public uint HardwareRevision { get; private set; } = 0;
        public string BundleCameraID = "";

        #region Update Device Infomation
        public event EventHandler<DevInfoUpdatedEventArgs> OnDevInfoUpdated;

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
        private void UpdatePhotoMetryInfo(byte[] param)
        {
            PhotometryInfo info = new PhotometryInfo();
            info.Model = BitConverter.ToUInt32(param.Take(4).ToArray(), 0);
            info.DetectorIdLength = BitConverter.ToUInt32(param.Skip(4).Take(4).ToArray(), 0);
            info.DetectorId = Encoding.ASCII.GetString(param.Skip(8).Take((int)info.DetectorIdLength).ToArray(), 0, (int)info.DetectorIdLength);
            int skipLength = 8 + (int)info.DetectorIdLength;
            info.SNlength = BitConverter.ToUInt32(param.Skip(skipLength).Take(4).ToArray(), 0);
            skipLength += 4;
            info.SN = Encoding.ASCII.GetString(param.Skip(skipLength).Take((int)info.SNlength).ToArray(), 0, (int)info.SNlength);
            skipLength += (int)info.SNlength;
            info.LightNumber = BitConverter.ToUInt32(param.Skip(skipLength).Take(4).ToArray(), 0);
            skipLength += 4;
            info.LightConfigInfos = new List<LightConfigInfo>();
            for (int i = 0; i < info.LightNumber; i++)
            {
                LightConfigInfo lightConfigInfo = new LightConfigInfo()
                {
                    ID = i,
                    WaveLength = BitConverter.ToUInt32(param.Skip(skipLength).Take(4).ToArray(), 0),
                    MaxPower = BitConverter.ToSingle(param.Skip(skipLength + 4).Take(4).ToArray(), 0)
                };
                skipLength += 8;
                info.LightConfigInfos.Add(lightConfigInfo);
                LightDesc ld = new LightDesc((uint)lightConfigInfo.ID, lightConfigInfo.WaveLength, lightConfigInfo.MaxPower);
                LightSourceList.Add(ld);
            }
            if (param.Length > skipLength)
            {
                info.IsOutput = param.Skip(skipLength).Take(1).First() == 1 ? true : false;
                skipLength += 1;
                info.OutputId = param.Skip(skipLength).Take(1).First();
            }

            PhotometryInfo = info;
        }
        public List<LightDesc> LightSourceList { get; private set; } = new List<LightDesc>();
        public PhotometryInfo PhotometryInfo { get; private set; } = new PhotometryInfo();
        private void UpdateIOConfig(byte[] param)
        {
            return;
        }
        #endregion

        #region Device Notification
        public event EventHandler<DevNotificationEventArgs> OnDevNotification;

        private List<DevInputNotificationEventArgs> DeviceInputUpdated(byte[] param)
        {
            try
            {
                if (param.Length < 8)
                {
                    return null;
                }
                List<DevInputNotificationEventArgs> devs = new List<DevInputNotificationEventArgs>();
                var length = BitConverter.ToUInt64(param.Skip(8).Take(8).ToArray(), 0);

                byte[] buffer = param.Skip(16).Take((int)length * 16).ToArray();
                var tamp = BitConverter.ToUInt64(buffer, 0);

                for (int i = 0; i < (int)length; i++)
                {
                    ulong io_timestamp = BitConverter.ToUInt64(param.Skip(16 * (i + 1)).Take(8).ToArray(), 0);
                    uint io_id = BitConverter.ToUInt32(param.Skip(16 * (i + 1) + 8).Take(4).ToArray(), 0);

                    uint io_stat = BitConverter.ToUInt32(param.Skip(16 * (i + 1) + 12).Take(4).ToArray(), 0);

                    DevInputNotificationEventArgs e = new DevInputNotificationEventArgs { IOID = io_id, Index = io_id, Status = io_stat, Timestamp = io_timestamp * 10 };
                    devs.Add(e);
                }

                return devs;
            }
            catch (Exception ex)
            {
                return null;
            }
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

                //case ProtocolIntent.INTENT_RETURN_LIGHT_CONFIG:
                //    UpdateLightConfig(e.Data);
                //    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                //    break;

                case ProtocolIntent.INTENT_RETURN_IO_CONFIG:
                    UpdateIOConfig(e.Data);
                    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                    break;
                case ProtocolIntent.INTENT_INPUT_IO:
                    //DevInputNotificationEventArgs ea = DeviceInputUpdated(e.Data);
                    //OnDevNotification?.Invoke(this, ea);
                    break;
                case ProtocolIntent.INTENT_PHOTOMETRY_INFO:
                    UpdatePhotoMetryInfo(e.Data);
                    OnDevInfoUpdated?.Invoke(this, new DevInfoUpdatedEventArgs());
                    break;
                case ProtocolIntent.INTENT_DEVICE_INFO_PUB:
                    break;
                default:
                    break;
            }
        }

        public void SetFrameRate(double frame_rate)
        {
            Console.WriteLine("frameRate:" + frame_rate);
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

        public void SendExposure(uint exposure)
        {
            CmdSendExposure cmd = new CmdSendExposure();
            cmd.SetCmdParam(exposure);
            _TC.Transmit(cmd);
            return;
        }
        public void SetGBLWF(List<WaverformStruct> waverforms)
        {
            waverforms.ToList().ForEach(x =>
            {
                CmdSetGBLWF cmd = new CmdSetGBLWF();
                cmd.SetCmdParam(x);
                _TC.Transmit(cmd);

            });
            return;
        }
        public void SetSweepState(int state)
        {
            CmdSetSweepState cmd = new CmdSetSweepState();
            cmd.SetCmdParam(state);
            _TC.Transmit(cmd);
            return;
        }
        public void SetCHNSweep(List<byte> datas)
        {
            CmdSetCHNSweep cmd = new CmdSetCHNSweep();
            cmd.SetCmdParam(datas);
            _TC.Transmit(cmd);
            return;
        }
        public void SetBindDio(List<byte> lightIds)
        {
            CmdSetBindDio cmd = new CmdSetBindDio();
            cmd.SetCmdParam(lightIds);
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
            //CmdOutputIO cmd = new CmdOutputIO();
            //cmd.SetCmdParam(io_id, stat);
            //_TC.Transmit(cmd);
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(io_id));
            b.AddRange(BitConverter.GetBytes(stat));
            _TC.OutputSend(b);
        }

        public void ADIsStartCollect(bool isStart)
        {
            _UARTA.IsStart = isStart;
        }
        public void SetAdframeRate(uint frameRate, uint[] array)
        {
            CmdAdframeRate cmd = new CmdAdframeRate();
            cmd.SetCmdParam(frameRate, array);
            _TC.Transmit(cmd);

            _UARTA.SetSampling((int)frameRate);
        }
        public void RemoveAdSampling()
        {
            _UARTA.RemoveSampling();
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
        #region 下位机升级
        public void GetDeviceInfo(uint type)
        {
            CmdDeviceInfo cmd = new CmdDeviceInfo();
            cmd.SetCmdParam(type);
            _TC.Transmit(cmd);
        }
        public void AppRun()
        {
            CmdAppRun cmdAppRun = new CmdAppRun();
            _TC.Transmit(cmdAppRun);
        }
        public void McuInit()
        {
            CmdInitMcu cmd = new CmdInitMcu();
            _TC.Transmit(cmd);
        }
        public void GetHashVal()
        {
            CmdGetHashVal cmd = new CmdGetHashVal();

            _TC.Transmit(cmd);
        }

        public void FileUpload(List<byte> bytes)
        {
            CmdSendFile cmd = new CmdSendFile();
            cmd.SetCmdParam(bytes);
            _TC.Transmit(cmd);
        }
        public void SetFileLength(UInt32 length)
        {
            CmdSetFileLength cmd = new CmdSetFileLength();
            cmd.SetCmdParam(length);
            _TC.Transmit(cmd);
        }
        #endregion
    }
}
