using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using InperProtocolStack.Communication;
using InperProtocolStack.Basis;
using InperProtocolStack.CmdPhotometry;
using System.Windows.Forms;

namespace InperProtocolStack.TransmissionCtrl
{
    public struct UsbAdDataStru
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public ushort[] Values;
    }
    public class TransTrafficCtrl
    {
        private UARTAgent _UARTA;
        private ReceivingProcessor _RProcessor = new ReceivingProcessor();
        public TransTrafficCtrl(UARTAgent uart_a)
        {
            _UARTA = uart_a;
            _RProcessor.OnMsgReceived += DevMsgReceived;
            _UARTA.OnDataReceived += new EventHandler<OnDataReceivedEventArgs>(OnDataReceived);
            _UARTA.OnDataReceivedUsb += UARTA_OnDataReceivedUsb;

            _ = Task.Run(() => { ReceivedDataMonitor(); });

            //SetupMetronome();
            _ = Task.Run(() => { TransmittingProcess(); });

            _ = Task.Run(() => { AdDataProcess(); });

            return;
        }
        private readonly Queue<byte[]> _AdDataCache = new Queue<byte[]>();
        private readonly object adDataCache = new object();

        private void UARTA_OnDataReceivedUsb(object sender, OnDataReceivedEventArgs e)
        {
            Monitor.Enter(adDataCache);
            _AdDataCache.Enqueue(e.Data);
            Monitor.Exit(adDataCache);
            //OnUsbInfoUpdated?.Invoke(this, e);
        }
        private unsafe void AdDataProcess()
        {
            while (true)
            {
                if (Monitor.TryEnter(adDataCache))
                {
                    if (_AdDataCache.Count > 0)
                    {
                        List<byte[]> vs = _AdDataCache.ToList();
                        _AdDataCache.Clear();
                        Monitor.Exit(adDataCache);

                        List<UsbAdData> usbAdDatas = new List<UsbAdData>();
                        foreach (byte[] data in vs)
                        {
                            fixed (byte* pb = &data[0])
                            {
                                
                                UsbAdDataStru res = Marshal.PtrToStructure<UsbAdDataStru>((IntPtr)pb);
                                //Console.WriteLine(res.Channel);
                                usbAdDatas.Add(new UsbAdData(res.Channel * 2 - 1, res.Time, res.Values.Select((v, i) =>
                                     {
                                         return i % 2 == 0 ? v : -1;
                                     }).Where(x => x != -1).ToList()));
                                usbAdDatas.Add(new UsbAdData(res.Channel * 2, res.Time, res.Values.Select((v, i) =>
                                   {
                                       return i % 2 == 1 ? v : -1;
                                   }).Where(x => x != -1).ToList()));
                            }
                        };
                        if (usbAdDatas.Count > 0)
                        {
                            OnUsbInfoUpdated?.Invoke(usbAdDatas);
                        }
                    }
                    else
                    {
                        Monitor.Exit(adDataCache);
                    }

                }
            }
        }

        public event EventHandler<MessageReceivedEventArgs> OnInfoUpdated;
        public event Action<List<UsbAdData>> OnUsbInfoUpdated;


        private void DevMsgReceived(object sender, MessageReceivedEventArgs e)
        {
            Confirm(e.Header.AckNumber);
            OnInfoUpdated?.Invoke(this, e);

            return;
        }


        private object _RCacheLock = new object();
        private Queue<byte> _ReceivingCache = new Queue<byte>();
        private AutoResetEvent _RecvAREvent = new AutoResetEvent(false);
        private void OnDataReceived(object sender, OnDataReceivedEventArgs e)
        {
            lock (_RCacheLock)
            {
                foreach (var d in e.Data)
                {
                    _ReceivingCache.Enqueue(d);
                }
            }
            _RecvAREvent.Set();

            return;
        }


        private void ReceivedDataMonitor()
        {
            while (true)
            {
                _RecvAREvent.WaitOne();
                if (Monitor.TryEnter(_RCacheLock))
                {
                    byte[] rd = _ReceivingCache.ToArray();
                    _ReceivingCache.Clear();
                    Monitor.Exit(_RCacheLock);

                    _RProcessor.AppendData(rd);
                }
            }
        }


        private uint _SendingSequence = 0;
        private object _CmdCacheLock = new object();
        private Queue<Command> _CommandCache = new Queue<Command>();
        private AutoResetEvent _AREvent = new AutoResetEvent(false);
        public void Transmit(Command cmd)
        {
            cmd.Sequence = _SendingSequence;
            _SendingSequence++;

            lock (_CmdCacheLock)
            {
                _CommandCache.Enqueue(cmd);
            }

            _AREvent.Set();

            return;
        }


        private bool CommandIsRedundant(Command cmd1, Command cmd2)
        {
            List<byte> intent_ss = new List<byte>
            {
                ProtocolIntent.INTENT_START,
                ProtocolIntent.INTENT_STOP
            };

            if (cmd1.Intent == cmd2.Intent || (intent_ss.Contains(cmd1.Intent) && intent_ss.Contains(cmd2.Intent)))
            {
                return true;
            }

            return false;
        }



        private void TransmittingProcess()
        {
            while (true)
            {
                _AREvent.WaitOne();


                Queue<Command> cq = new Queue<Command>();
                lock (_CmdCacheLock)
                {
                    cq = new Queue<Command>(_CommandCache);
                    _CommandCache.Clear();
                }

                if (cq.Count == 0)
                {
                    continue;
                }

                Command cmd_ready = cq.Dequeue();
                while (cq.Count != 0)
                {
                    Command cmd_top = cq.Dequeue();
                    if (CommandIsRedundant(cmd_ready, cmd_top))
                    {
                        cmd_ready = cmd_top;
                        continue;
                    }

                    _ = _UARTA.SendDataUsb(cmd_ready.GetBytes());
                    //_UARTA.SendData(cmd_ready.GetBytes());
                    cmd_ready = cmd_top;
                    Thread.Sleep(20);
                }

                _ = _UARTA.SendDataUsb(cmd_ready.GetBytes());
                //_UARTA.SendData(cmd_ready.GetBytes());
                Thread.Sleep(20);
            }
        }


        private void Confirm(uint sequence)
        {
            //System.Diagnostics.Debug.WriteLine("Command {0} confirmed.", sequence);
            return;
        }


        //private System.Timers.Timer _WaitTimeoutTimer = new System.Timers.Timer();
        private void SetupMetronome()
        {
            _AREvent.Set();
            //_WaitTimeoutTimer.Interval = 1000;
            //_WaitTimeoutTimer.AutoReset = false;
            //_WaitTimeoutTimer.Enabled = false;
            //_WaitTimeoutTimer.Elapsed += OnTimeout;
        }


        //private void OnTimeout(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    return;
        //}
    }
}
