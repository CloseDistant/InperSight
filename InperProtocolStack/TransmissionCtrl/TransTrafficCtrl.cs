using InperProtocolStack.Basis;
using InperProtocolStack.CmdPhotometry;
using InperProtocolStack.Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace InperProtocolStack.TransmissionCtrl
{
    public struct UsbAdDataStru512
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public short[] Values;
    }
    public struct UsbAdDataStru256
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public short[] Values;
    }
    public struct UsbAdDataStru128
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public short[] Values;
    }
    public struct UsbAdDataStru64
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public short[] Values;
    }
    public struct UsbAdDataStru32
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public short[] Values;
    }
    public struct UsbAdDataStru16
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public short[] Values;
    }
    public struct UsbAdDataStru8
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] Values;
    }
    public struct UsbAdDataStru4
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] Values;
    }
    public struct UsbAdDataStru2
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public short[] Values;
    }
    public struct UsbAdDataStru1
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Channel;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint Time;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public byte[] Fill;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public short[] Values;
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
            //_UARTA.OnDataReceivedUsb += UARTA_OnDataReceivedUsb;
            _UARTA.OnInputReceived += _UARTA_OnInputReceived;
            _ = Task.Run(() => { ReceivedDataMonitor(); });

            //SetupMetronome();
            _ = Task.Run(() => { TransmittingProcess(); });


            return;
        }
        public event EventHandler<byte[]> OnInputUpdated;
        private void _UARTA_OnInputReceived(object sender, byte[] e)
        {
            OnInputUpdated?.Invoke(sender, e);
        }

        public event EventHandler<MessageReceivedEventArgs> OnInfoUpdated;

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
        private ConcurrentQueue<Command> _CommandCache = new ConcurrentQueue<Command>();
        private AutoResetEvent _AREvent = new AutoResetEvent(false);
        bool isFirst = true;
        public void Transmit(Command cmd)
        {
            cmd.Sequence = _SendingSequence;
            _SendingSequence++;

            //lock (_CmdCacheLock)
            //{
            _CommandCache.Enqueue(cmd);
            //}

            if (isFirst || _SendingSequence > sequence + 1)
            {
                TransmittingProcess();
                isFirst = false;
            }

            return;
        }
        public void OutputSend(List<byte> b)
        {
            _ = _UARTA.SendDataUsb(b, 2);
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


        int _pLock = 0;
        private void TransmittingProcess()
        {
            if (Interlocked.Exchange(ref _pLock, 1) == 0)
            {
                if (_CommandCache.TryDequeue(out Command cmd))
                {
                    Console.WriteLine(BitConverter.ToString(cmd.GetBytes().ToArray()));
                    _ = _UARTA.SendDataUsb(cmd.GetBytes(), 1);
                }

                if (_CommandCache.Count == 0)
                {
                    isFirst = true;
                }
                Interlocked.Exchange(ref _pLock, 0);
            }
        }

        private int sequence = 0;
        private void Confirm(uint sequence)
        {
            this.sequence = (int)sequence;
            Console.WriteLine("收到回复" + sequence);
            Task.Run(() => { TransmittingProcess(); });

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
