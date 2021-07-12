using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudioControlLib.Lib.DeviceAgency.ControlDept
{
    public class PortStatusChangedEventArgs : EventArgs
    {
        private byte[] _PortStatus;
        public PortStatusChangedEventArgs(byte[] status)
        {
            _PortStatus = status;
        }
        public byte[] PortStatus => _PortStatus;
    }


    public class LightStatusChangedEventArgs : EventArgs
    {
        public short Light0WaveLength;
        public short Light1WaveLength;
        public short Light2WaveLength;
        public short Light3WaveLength;
    }


    class USARTOperator
    {
        private static readonly USARTOperator _Instance = new USARTOperator();

        public static USARTOperator Instance => _Instance;

        private SerialPortAgent _SPA = SerialPortAgent.Instance;
        //public SerialPortAgent SPA => _SPA;


        public event EventHandler<PortStatusChangedEventArgs> PortStatusChanged;
        public void RaisePortStatusChangedEvent(byte[] status)
        {
            PortStatusChanged?.Invoke(this, new PortStatusChangedEventArgs(status));
        }

        public event EventHandler<LightStatusChangedEventArgs> LightStatusChanged;
        public void RaiseLightStatusChangedEvent()
        {
            LightStatusChanged?.Invoke(this, new LightStatusChangedEventArgs() 
            {
                Light0WaveLength = this.Light0WaveLength,
                Light1WaveLength = this.Light1WaveLength,
                Light2WaveLength = this.Light2WaveLength,
                Light3WaveLength = this.Light3WaveLength
            });
        }


        private USARTOperator()
        {
            _SPA.OnDataReceived += new EventHandler<OnDataReceivedEventArgs>(OnDataReceived);
        }


        private byte[] ExpansionByteToByteArray(byte b)
        {
            string strResult = Convert.ToString(b, 2);
            strResult = strResult.Insert(0, new string('0', 8 - strResult.Length));

            byte[] r = { 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < 8; i++)
            {
                r[7 - i] = byte.Parse(strResult.Substring(i, 1));
            }
            return r;
        }



        private void ReportDeviceInput(byte input_status)
        {
            byte[] r = ExpansionByteToByteArray((byte)input_status);
            RaisePortStatusChangedEvent(r);
            //System.Diagnostics.Debug.WriteLine(">> " + string.Join(",", r)); ;
            return;
        }

        public short Light0WaveLength { get; private set; } = 0;
        public short Light1WaveLength { get; private set; } = 0;
        public short Light2WaveLength { get; private set; } = 0;
        public short Light3WaveLength { get; private set; } = 0;

        private void SetupLightWaveLength(byte[] data)
        {
            Light0WaveLength = BitConverter.ToInt16(data.Take(2).ToArray(), 0);
            Light1WaveLength = BitConverter.ToInt16(data.Skip(2).Take(2).ToArray(), 0);
            Light2WaveLength = BitConverter.ToInt16(data.Skip(4).Take(2).ToArray(), 0);
            Light3WaveLength = BitConverter.ToInt16(data.Skip(6).Take(2).ToArray(), 0);

            RaiseLightStatusChangedEvent();

            return;
        }


        private void OnDataReceived(object sender, OnDataReceivedEventArgs e)
        {
            List<byte> buf = new List<byte>(e.Data);

            while (buf.Count != 0)
            {
                byte header = buf.First();
                switch (header)
                {
                    case 0x06:
                        SetupLightWaveLength(buf.Skip(1).ToArray());
                        buf = buf.Skip(5).ToList();
                        break;

                    case 0xff:
                        ReportDeviceInput(buf[1]);
                        buf = buf.Skip(2).ToList();
                        break;

                    default:
                        buf = buf.Skip(1).ToList();
                        break;
                }
            }
        }

        public void SetLightPower(int light_no, double power_in_percent)
        {
            short e_power = (short)((power_in_percent * 4095) / 100);
            byte[] data = BitConverter.GetBytes(e_power);

            switch (light_no)
            {
                case 0:
                    data[1] |= 0xa0;
                    break;

                case 1:
                    data[1] |= 0xd0;
                    break;

                case 2:
                    data[1] |= 0x40;
                    break;

                case 3:
                    data[1] |= 0x50;
                    break;

                default:
                    break;
            }

            _SPA.SendData(new List<byte>(data));

            return;
        }


        public void SwitchLight(int light_no, bool enable)
        {
            short dual_light_enable = (short)(enable ? 1 : 0);
            byte[] data = BitConverter.GetBytes(dual_light_enable);
            switch (light_no)
            {
                case 0:
                    data[1] |= 0x10;
                    break;

                case 1:
                    data[1] |= 0xc0;
                    break;

                case 2:
                    data[1] |= 0x20;
                    break;

                case 3:
                    data[1] |= 0x30;
                    break;

                default:
                    break;
            }

            _SPA.SendData(new List<byte>(data));

            return;
        }


        public void GetDeviceConfig()
        {
            byte[] data = new byte[2] { 0, 0 };
            data[1] |= 0x60;
            _SPA.SendData(new List<byte>(data));
            return;
        }

        public void FlashConfig()
        {
            byte[] data = new byte[2] { 0, 0 };
            data[1] |= 0x80;
            _SPA.SendData(new List<byte>(data));
            return;
        }

        public void ConfigLightWaveLength(byte light_index, short wave_length)
        {
            byte[] wl = BitConverter.GetBytes(wave_length);
            byte head = 0x70;
            head |= light_index;

            List<byte> data = new List<byte>(wl);
            data.Add(head);

            _SPA.SendData(data);

            return;
        }


        public double SetFrameRate(double frame_rate)
        {
            short frs = (short)(frame_rate + 1);
            byte[] data = BitConverter.GetBytes(frs);
            data[1] |= 0xb0;
            _SPA.SendData(new List<byte>(data));

            return frame_rate;
        }


        public void SetOutputStatus(byte o_port_status)
        {
            byte[] data = new byte[2] { o_port_status, 0 };
            data[1] |= 0xe0;
            _SPA.SendData(new List<byte>(data));
            return;
        }


        public void ResetDevice()
        {
            for (int i = 0; i < 4; i++)
            {
                SetLightPower(i, 0);
                System.Threading.Thread.Sleep(5);
                SwitchLight(i, false);
                System.Threading.Thread.Sleep(5);
            }

            SetFrameRate(30);

            return;
        }
    }
}
