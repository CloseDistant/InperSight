using InperProtocolStack.Basis;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InperProtocolStack.TransmissionCtrl
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private FrameHeader _Header;
        private byte[] _Data;
        public MessageReceivedEventArgs(FrameHeader header, byte[] data)
        {
            _Header = header;
            _Data = data;
        }

        public byte Intent => _Header.Intent;
        public FrameHeader Header => _Header;
        public byte[] Data => _Data;
    }


    enum ReceivingState
    {
        RS_WAITING_HEADER,
        RS_RECEIVING_HEADER,
        RS_WAITING_TAIL
    }


    enum ReceivingTrigger
    {
        RT_HEADER_MAGIC_FRONT_APPEARANCE,
        RT_HEADER_MAGIC_APPEARANCE,

        RT_GET_VALID_HEADER,
        RT_GET_INVALID_HEADER,

        RT_TAIL_MAGIC_APPEARANCE,
        RT_GET_VALID_BODY,
        RT_GET_INVALID_BODY,

        RT_RECEIVING_TIMEOUT,

        RT_NONE
    }


    public class ReceivingProcessor
    {
        private StateMachine<ReceivingState, ReceivingTrigger> _FSM
            = new StateMachine<ReceivingState, ReceivingTrigger>(ReceivingState.RS_WAITING_HEADER, FiringMode.Queued);


        #region Receiving Caches
        private object _ReceivingQLock = new object();
        private Queue<byte> _ReceivingQ = new Queue<byte>();
        #endregion


        #region Notification
        public event EventHandler<MessageReceivedEventArgs> OnMsgReceived;
        #endregion


        #region Frame Re-Structure
        private void OnFrameReveived(FrameHeader header, byte[] body)
        {
            OnMsgReceived?.Invoke(this, new MessageReceivedEventArgs(header, body));
        }
        #endregion


        #region Interpret Process
        private readonly byte[] _HeaderMagic = BitConverter.GetBytes(FrameHeader.HEAD_MAGIC);
        private readonly byte[] _TailMagic = BitConverter.GetBytes(FrameTail.TAIL_MAGIC);
        private readonly int _HeaderLength = Marshal.SizeOf(typeof(FrameHeader.StructFrameHeader));
        private readonly int _TailLenth = 4;

        private byte[] _InterpretBuffer;
        private byte[] _CompleteDataFrame;
        private AutoResetEvent _AREInterpretLamp = new AutoResetEvent(false);
        private ReceivingTrigger CkWaitingHeaderEvt()
        {
            if (_InterpretBuffer == null || _InterpretBuffer.Length == 0)
            {
                return ReceivingTrigger.RT_NONE;
            }

            ReceivingTrigger trg = ReceivingTrigger.RT_NONE;

            if (_InterpretBuffer.Length == 1 && _InterpretBuffer[0] == _HeaderMagic[0])
            {
                return ReceivingTrigger.RT_HEADER_MAGIC_FRONT_APPEARANCE;
            }
            else if (_InterpretBuffer.Length > 1 &&
                _InterpretBuffer[0] == _HeaderMagic[0] &&
                _InterpretBuffer[1] == _HeaderMagic[1])
            {
                return ReceivingTrigger.RT_HEADER_MAGIC_APPEARANCE;
            }
            else if (_TimeoutCount > _MaxTimeout)
            {
                return ReceivingTrigger.RT_RECEIVING_TIMEOUT;
            }
            else
            {
                return ReceivingTrigger.RT_NONE;
            }

        }


        private ReceivingTrigger CkReceivingHeaderEvt()
        {
            if (_InterpretBuffer.Length >= _HeaderLength)
            {
                var r_header = Utils.BytesToStruct<FrameHeader.StructFrameHeader>(_InterpretBuffer, _HeaderLength);
                var r_crc = r_header.CRC;

                byte[] header_except_crc = _InterpretBuffer.Take(_HeaderLength - 2).ToArray();
                var c_crc = BitConverter.ToUInt16(CRCCalculation.CRC16(header_except_crc), 0);

                if (r_crc == c_crc)
                {
                    return ReceivingTrigger.RT_GET_VALID_HEADER;
                }
                else
                {
                    return ReceivingTrigger.RT_GET_INVALID_HEADER;
                }
            }
            else if (_TimeoutCount > _MaxTimeout)
            {
                return ReceivingTrigger.RT_RECEIVING_TIMEOUT;
            }
            else
            {
                return ReceivingTrigger.RT_NONE;
            }
        }


        private ReceivingTrigger CKWaitingTailEvt()
        {
            var r_header = Utils.BytesToStruct<FrameHeader.StructFrameHeader>(_InterpretBuffer, _HeaderLength);
            var body_len = r_header.BodyLength;
            int frame_len = _HeaderLength + body_len + _TailLenth;

            if (_InterpretBuffer.Length >= frame_len)
            {
                byte[] tail_bytes = _InterpretBuffer.Skip(_HeaderLength + body_len).Take(_TailLenth).ToArray();
                var r_tail = Utils.BytesToStruct<FrameTail.StructFrameTail>(tail_bytes, _TailLenth);
                var r_body_crc = r_tail.CRC;

                ushort c_body_crc = 0;
                if (body_len > 0)
                {
                    byte[] body_bytes = _InterpretBuffer.Skip(_HeaderLength).Take(body_len).ToArray();
                    c_body_crc = BitConverter.ToUInt16(CRCCalculation.CRC16(body_bytes), 0);
                }

                if (r_body_crc == c_body_crc && r_tail.TailMagic == BitConverter.ToUInt16(_TailMagic, 0))
                {
                    _CompleteDataFrame = _InterpretBuffer.Take(frame_len).ToArray();
                    return ReceivingTrigger.RT_GET_VALID_BODY;
                }
                else
                {
                    return ReceivingTrigger.RT_GET_INVALID_BODY;
                }

            }
            else if (_TimeoutCount > _MaxTimeout)
            {
                return ReceivingTrigger.RT_RECEIVING_TIMEOUT;
            }
            else
            {
                return ReceivingTrigger.RT_NONE;
            }
        }


        private void InterpretProcess()
        {
            while (true)
            {
                _AREInterpretLamp.WaitOne();
                Queue<byte> interpret_q = new Queue<byte>();

                if (Monitor.TryEnter(_ReceivingQLock))
                {
                    interpret_q = new Queue<byte>(_ReceivingQ);
                    _ReceivingQ.Clear();
                    Monitor.Exit(_ReceivingQLock);
                }
                else
                {
                    continue;
                }

                //while (interpret_q.Count > 0 && interpret_q.First() != _HeaderMagic[0])
                //{
                //    _ = interpret_q.Dequeue();
                //}

                Queue<byte> swap_q = new Queue<byte>();
                while (interpret_q.Count > 0)
                {
                    swap_q.Enqueue(interpret_q.Dequeue());
                    _InterpretBuffer = swap_q.ToArray();

                    ReceivingTrigger trg = ReceivingTrigger.RT_NONE;
                    switch (_FSM.State)
                    {
                        case ReceivingState.RS_RECEIVING_HEADER:
                            trg = CkReceivingHeaderEvt();
                            break;

                        case ReceivingState.RS_WAITING_HEADER:
                            trg = CkWaitingHeaderEvt();
                            break;

                        case ReceivingState.RS_WAITING_TAIL:
                            trg = CKWaitingTailEvt();
                            break;

                        default:
                            break;
                    }

                    _FSM.Fire(trg);
                }
            }
        }
        #endregion


        #region actions
        private void ActionClearBuffer()
        {
            lock (_ReceivingQLock)
            {
                _ReceivingQ.Clear();
                _InterpretBuffer = null;
            }
            _TimeoutCount = 0;
            return;
        }

        private void ActionDisposalInvalidHeader() { }
        private void ActionDisposalInvalidBody() { }

        private void ActionNotifyHeader() { }


        private void ActionNotifyFrame()
        {
            var s_header = Utils.BytesToStruct<FrameHeader.StructFrameHeader>(_CompleteDataFrame, _HeaderLength);
            FrameHeader header = new FrameHeader(s_header);

            var body_len = header.BodyLength;
            byte[] body_bytes = null;
            if (body_len > 0)
            {
                body_bytes = _CompleteDataFrame.Skip(_HeaderLength).Take(body_len).ToArray();
            }
            OnFrameReveived(header, body_bytes);

            ActionClearBuffer();

            return;
        }
        #endregion


        private uint _TimeoutCount = 0;
        private object _TimeoutCountingLock = new object();
        private readonly uint _MaxTimeout = 6;
        private System.Timers.Timer _ReceivingTimer = new System.Timers.Timer();
        private void SetReceivingTimer()
        {
            _ReceivingTimer.Interval = 500;
            _ReceivingTimer.AutoReset = true;
            _ReceivingTimer.Enabled = false;
            _ReceivingTimer.Elapsed += OnTimeout;
        }


        private void OnTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(_TimeoutCountingLock))
            {
                _TimeoutCount += 1;
                Monitor.Exit(_TimeoutCountingLock);
            }
        }


        public ReceivingProcessor()
        {
            _FSM.Configure(ReceivingState.RS_WAITING_HEADER)
                .Permit(ReceivingTrigger.RT_HEADER_MAGIC_APPEARANCE, ReceivingState.RS_RECEIVING_HEADER)
                .InternalTransition(ReceivingTrigger.RT_RECEIVING_TIMEOUT, t => ActionClearBuffer())
                .InternalTransition(ReceivingTrigger.RT_NONE, t => ActionClearBuffer())
                .OnEntryFrom(ReceivingTrigger.RT_GET_INVALID_HEADER, t => ActionDisposalInvalidHeader())
                .OnEntryFrom(ReceivingTrigger.RT_GET_VALID_BODY, t => ActionNotifyFrame())
                .OnEntryFrom(ReceivingTrigger.RT_GET_INVALID_BODY, t => ActionDisposalInvalidBody())
                .OnEntryFrom(ReceivingTrigger.RT_RECEIVING_TIMEOUT, t => ActionClearBuffer());

            _FSM.Configure(ReceivingState.RS_RECEIVING_HEADER)
                .Permit(ReceivingTrigger.RT_GET_VALID_HEADER, ReceivingState.RS_WAITING_TAIL)
                .Permit(ReceivingTrigger.RT_GET_INVALID_HEADER, ReceivingState.RS_WAITING_HEADER);

            _FSM.Configure(ReceivingState.RS_WAITING_TAIL)
                .Permit(ReceivingTrigger.RT_GET_VALID_BODY, ReceivingState.RS_WAITING_HEADER)
                .Permit(ReceivingTrigger.RT_GET_INVALID_BODY, ReceivingState.RS_WAITING_HEADER)
                .Permit(ReceivingTrigger.RT_RECEIVING_TIMEOUT, ReceivingState.RS_WAITING_HEADER)
                .OnEntryFrom(ReceivingTrigger.RT_GET_VALID_HEADER, t => ActionNotifyHeader());

            _FSM.OnUnhandledTrigger((state, trigger) => { });

            Task.Run(() => { InterpretProcess(); });

            SetReceivingTimer();
        }


        public void AppendData(byte[] data)
        {
            lock (_ReceivingQLock)
            {
                foreach (var d in data)
                {
                    _ReceivingQ.Enqueue(d);
                }
            }
            lock (_TimeoutCountingLock)
            {
                _TimeoutCountingLock = 0;
            }
            _AREInterpretLamp.Set();

            return;
        }
    }
}
