using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperProtocolStack.Basis
{
    public class FrameHeader
    {
        public const ushort HEAD_MAGIC = 0x5748;


        public struct StructFrameHeader
        {
            public ushort HeaderMagic;
            public byte Revision;
            public byte Intent;
            public uint SequenceNumber;
            public uint AckNumber;
            public ushort BodyLength;
            public ushort CRC;
        }


        private StructFrameHeader _Header = new StructFrameHeader();


        private void UpdateHeaderCRC()
        {
            List<byte> byte_header = Utils.StructToBytes(_Header);

            byte[] header_except_crc = byte_header.Take(byte_header.Count - 2).ToArray();

            ushort crc = BitConverter.ToUInt16(CRCCalculation.CRC16(header_except_crc), 0);
            _Header.CRC = crc;

            return;
        }


        public byte Intent
        {
            get
            {
                return _Header.Intent;
            }
            set
            {
                _Header.Intent = value;
                UpdateHeaderCRC();
            }
        }


        public UInt32 Sequence
        {
            get
            {
                return _Header.SequenceNumber;
            }
            set
            {
                _Header.SequenceNumber = value;
                UpdateHeaderCRC();
            }
        }


        public UInt32 AckNumber
        {
            get
            {
                return _Header.AckNumber;
            }
            set
            {
                _Header.AckNumber = value;
                UpdateHeaderCRC();
            }
        }


        public UInt16 BodyLength
        {
            get
            {
                return _Header.BodyLength;
            }
            set
            {
                _Header.BodyLength = value;
                UpdateHeaderCRC();
            }
        }


        public UInt16 CRC
        {
            get
            {
                return _Header.CRC;
            }
        }


        public FrameHeader()
        {
            _Header.HeaderMagic = HEAD_MAGIC;
            _Header.Revision = 1;
            _Header.Intent = 0;
            _Header.SequenceNumber = 0;
            _Header.AckNumber = 0;
            _Header.BodyLength = 0;

            UpdateHeaderCRC();
        }


        public FrameHeader(StructFrameHeader s_header)
        {
            _Header = s_header;
        }


        public List<byte> GetBytes()
        {
            return Utils.StructToBytes(_Header);
        }
    }


    public class FrameTail
    {
        public const ushort TAIL_MAGIC = 0xFECA;

        public struct StructFrameTail
        {
            public ushort CRC;
            public ushort TailMagic;
        }

        private StructFrameTail _Tail = new StructFrameTail()
        {
            CRC = 0,
            TailMagic = TAIL_MAGIC
        };

        public ushort CRC => _Tail.CRC;


        public FrameTail(ushort crc)
        {
            _Tail.CRC = crc;
            return;
        }


        public List<byte> GetBytes()
        {
            List<byte> tail = new List<byte>();
            tail.AddRange(BitConverter.GetBytes(CRC));
            tail.AddRange(BitConverter.GetBytes(TAIL_MAGIC));

            return tail;
        }
    }


    public class DataFrame
    {
        private FrameHeader _Header = new FrameHeader();
        public FrameHeader Header
        {
            get
            {
                return _Header;
            }
            set
            {
                _Header = value;
                _Header.BodyLength = (ushort)_FrameBody.Count;
            }
        }


        public FrameTail Tail { get; private set; } = new FrameTail(0);


        private List<byte> _FrameBody = new List<byte>();
        public List<byte> FrameBody
        {
            get
            {
                return _FrameBody;
            }

            set
            {
                _FrameBody = value;
                Header.BodyLength = (ushort)_FrameBody.Count;
                ushort crc = BitConverter.ToUInt16(CRCCalculation.CRC16(_FrameBody.ToArray()), 0);
                Tail = new FrameTail(crc);
            }
        }


        public DataFrame(FrameHeader header, List<byte> body)
        {
            Header = header;
            FrameBody = body;
        }


        public List<byte> GetBytes()
        {
            List<byte> header = Header.GetBytes();
            List<byte> body = FrameBody;
            List<byte> tail = Tail.GetBytes();

            List<byte> frame = new List<byte>();
            frame.AddRange(header);
            frame.AddRange(body);
            frame.AddRange(tail);

            return frame;
        }
    }
}
