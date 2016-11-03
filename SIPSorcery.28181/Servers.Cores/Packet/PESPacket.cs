using GLib.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.Servers.Cores.Packet
{
    public class PESPacket : IByteObj
    {

        public byte[] Packet_Start_Code_Prefix;     // 3 bytes
        public byte Stream_ID;					    // 1 byte
        public ushort PES_Packet_Length;			// 2 bytes

        public byte[] PES_Header_Flags;			    // 2 bytes = "10"(2 biti) + PES_H_F(14 biti)
        public byte PES_Header_Length;			    // 1 byte
        public byte[] PES_Header_Fields;			// Variable length

        public byte[] PES_Packet_Data;			    // Variable length
        private MediaFrame _MediaFrame = null;
        private bool _isVideo = false;

        public unsafe MediaFrame MediaFrame
        {
            get { return _MediaFrame; }
            set
            {
                try
                {
                    _MediaFrame = value;
                    if (_MediaFrame == null)
                        throw new Exception();

                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        public PESPacket()
        {
            Packet_Start_Code_Prefix = new byte[] { 0x00, 0x00, 0x01 };
            //以几项写死，可能有些情况不能写死
            PES_Header_Flags = new byte[2] { 0x81, 0xC0 };//PTS_DTS_flags 11 头扩展信息中包含 pts和dts
            PES_Header_Length = 10;           //信息区长度
            PES_Header_Fields = new byte[10];
        }

        public unsafe PESPacket(byte[] packetData, bool IsVideo, long timetick)
        {
            _isVideo = IsVideo;

            Packet_Start_Code_Prefix = new byte[] { 0x00, 0x00, 0x01 };
            //以几项写死，可能有些情况不能写死
            PES_Header_Flags = new byte[2] { 0x81, 0xC0 };//PTS_DTS_flags 11 头扩展信息中包含 pts和dts

            PES_Header_Length = 10;                         //信息区长度
            PES_Header_Fields = new byte[10];
            if (!IsVideo && false)
            {
                PES_Header_Flags = new byte[2] { 0x80, 0x80 };//PTS_DTS_flags 11 头扩展信息中包含 pts和dts
                PES_Header_Length = 5;
                PES_Header_Fields = new byte[5];
            }
            Stream_ID = (byte)(IsVideo ? 0xE0 : 0xC0);
            SetTimetick(PES_Header_Fields, timetick, timetick);
            PES_Packet_Data = packetData;
            int packet_len = (3 + PES_Header_Length + PES_Packet_Data.Length);
            PES_Packet_Length = (ushort)packet_len;
            if (packet_len > 65526)
            {
                //如果h264的包大于 65535  的话，可以设置PES_packet_length为0 ，具体参见ISO/ICE 13818-1.pdf  49 / 174 中关于PES_packet_length的描述
                //打包PES, 直接读取一帧h264的内容, 此时我们设置PES_packet_length的值为0000
                //表示不指定PES包的长度,ISO/ICE 13818-1.pdf 49 / 174 有说明,这主要是方便
                //当一帧H264的长度大于PES_packet_length(2个字节)能表示的最大长度65535
                //的时候分包的问题, 这里我们设置PES_packet_length的长度为0000之后 ,  那么即使该H264视频帧的长度
                //大于65535个字节也不需要分多个PES包存放, 事实证明这样做是可以的, ipad可播放
                PES_Packet_Length = 0;
            }
        }

        private unsafe byte[] GetHeadBytes()
        {
            byte[] head = new byte[3 + PES_Header_Length];
            head[0] = PES_Header_Flags[0];//0x81
            head[1] = PES_Header_Flags[1];//PTS_DTS_flags 11 头扩展信息中包含 pts和dts
            head[2] = PES_Header_Length;           //信息区长度
            Array.Copy(PES_Header_Fields, 0, head, 3, PES_Header_Length);
            return head;
        }

        private unsafe void SetTimetick(byte[] buf, long pts, long dts)
        {
            buf[0] = (byte)(((pts >> 29) | 0x31) & 0x3f);
            buf[1] = (byte)(pts >> 22);
            buf[2] = (byte)((pts >> 14) | 0x01);
            buf[3] = (byte)(pts >> 7);
            buf[4] = (byte)((pts << 1) | 0x01);
            if (PES_Header_Length > 5)
            {
                buf[5] = (byte)(((dts >> 29) | 0x11) & 0x1f);
                buf[6] = (byte)(dts >> 22);
                buf[7] = (byte)((dts >> 14) | 0x01);
                buf[8] = (byte)(dts >> 7);
                buf[9] = (byte)((dts << 1) | 0x01);
            }
        }

        private unsafe long ParsePTS(byte* pBuf)
        {
            long llpts = (((uint)(pBuf[0] & 0x0E)) << 29)
               | (uint)(pBuf[1] << 22)
               | (((uint)(pBuf[2] & 0xFE)) << 14)
               | (uint)(pBuf[3] << 7)
               | (uint)(pBuf[4] >> 1);
            return llpts;
        }

        public unsafe long GetVideoTimetick()
        {
            fixed (byte* pbuf = PES_Header_Fields)
            {
                return ParsePTS(pbuf) / 90;
            }
        }

        private byte[] GetBodyBytes()
        {
            if (MediaFrame != null)
            {
                return MediaFrame.Data;
            }
            else if (PES_Packet_Data != null)
                return PES_Packet_Data;
            else
                return null;

        }

        public byte[] GetBytes()
        {
            var head = GetHeadBytes();
            var body = GetBodyBytes();
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            bw.Write(Packet_Start_Code_Prefix);
            bw.Write(Stream_ID);
            bw.Write(IPAddress.HostToNetworkOrder((short)PES_Packet_Length));
            bw.Write(head);
            bw.Write(body);
            return ms.ToArray();
        }

        public void SetBytes(Stream stream)
        {
            var br = new System.IO.BinaryReader(stream);
            Packet_Start_Code_Prefix = br.ReadBytes(3);
            if (Packet_Start_Code_Prefix[0] != 0 || Packet_Start_Code_Prefix[1] != 0 || Packet_Start_Code_Prefix[2] != 1)
                throw new Exception();
            Stream_ID = br.ReadByte();
            PES_Packet_Length = (ushort)IPAddress.NetworkToHostOrder(br.ReadInt16());
            if (PES_Packet_Length == 0 && false)
            {
                throw new Exception("PES_Packet_Length error");
            }
            PES_Header_Flags = br.ReadBytes(2);
            PES_Header_Length = br.ReadByte();
            PES_Header_Fields = br.ReadBytes(PES_Header_Length);
            if (PES_Packet_Length > 0)
            {
                PES_Packet_Data = br.ReadBytes(PES_Packet_Length - 3 - PES_Header_Length);
                if (PES_Packet_Data.Length < PES_Packet_Length - 3 - PES_Header_Length)
                {
                    Console.WriteLine("@@@@@@@@@@@@@@@" + PES_Packet_Length);
                }
            }
            else
            {
                PES_Packet_Data = br.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public void SetBytes(byte[] buf)
        {
            var ms = new System.IO.MemoryStream(buf);
            SetBytes(ms);
        }
    }
}
