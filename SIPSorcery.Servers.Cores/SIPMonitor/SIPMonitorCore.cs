using SIPSorcery.Net;
using SIPSorcery.Servers.Cores.Packet;
using SIPSorcery.Servers.SIPMessage;
using SIPSorcery.SIP;
using SIPSorcery.Sys.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.Servers.SIPMonitor
{
    /// <summary>
    /// sip监控核心处理
    /// </summary>
    public class SIPMonitorCore : ISIPMonitorService
    {
        private SIPMessageCore _msgCore;
        /// <summary>
        /// rtp数据通道
        /// </summary>
        private RTPChannel _rtpChannel;
        private string _deviceId;
        private string _deviceName;
        private TaskTiming _realTask;
        private TaskTiming _byeTask;
        private SIPRequest _realReqSession;

        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<string, SipServiceStatus> OnSIPServiceChanged;

        public SIPMonitorCore(SIPMessageCore msgCore, string deviceId, string name)
        {
            _msgCore = msgCore;
            _deviceId = deviceId;
            _deviceName = name;
        }

        private FileStream m_fs = null;

        private void _rtpChannel_OnFrameReady(RTPFrame frame)
        {
            byte[] buffer = frame.GetFramePayload();
            Write(buffer);
        }

        public void OnSIPServiceChange(string msg, SipServiceStatus state)
        {
            Action<string, SipServiceStatus> action = OnSIPServiceChanged;

            if (action == null) return;

            foreach (Action<string, SipServiceStatus> handler in action.GetInvocationList())
            {
                try { handler(msg, state); }
                catch { continue; }
            }
        }

        /// <summary>
        /// 实时视频请求
        /// </summary>
        /// <param name="deviceId">设备编码</param>
        public void RealVideoReq()
        {
            if (_msgCore.LocalEndPoint == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }

            int[] mediaPort = _msgCore.SetMediaPort();

            string localIp = _msgCore.LocalEndPoint.Address.ToString();

            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();

            SIPRequest realReq = RealVideoReq(localIp, mediaPort, fromTag, cSeq, callId);
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, realReq);
            //_realTask = new TaskTiming(realReq, _msgCore.Transport);
            //_msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            //_realTask.Start();

            _rtpChannel = new RTPChannel(_msgCore.RemoteEndPoint.GetIPEndPoint(), mediaPort[0], mediaPort[1], FrameTypesEnum.H264);
            _rtpChannel.OnFrameReady += _rtpChannel_OnFrameReady;
            _rtpChannel.Start();
        }

        /// <summary>
        /// 确认接收视频请求
        /// </summary>
        /// <param name="response">响应消息</param>
        /// <returns></returns>
        public SIPRequest AckRequest(SIPResponse response)
        {
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPRequest ackReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.ACK, remoteUri);
            SIPFromHeader from = new SIPFromHeader(null, response.Header.From.FromURI, response.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, response.Header.To.ToTag);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            SIPHeader header = new SIPHeader(from, to, response.Header.CSeq, response.Header.CallId);
            header.CSeqMethod = SIPMethodsEnum.ACK;
            header.Contact = response.Header.Contact;
            header.Contact.Clear();
            header.Contact.Add(contactHeader);
            header.Vias = response.Header.Vias;
            header.MaxForwards = response.Header.MaxForwards;
            header.ContentLength = response.Header.ContentLength;
            header.UserAgent = _msgCore.UserAgent;
            header.Allow = null;
            ackReq.Header = header;
            return ackReq;
        }

        private SIPRequest RealVideoReq(string localIp, int[] mediaPort, string fromTag, int cSeq, string callId)
        {
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest realReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INVITE, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            realReq.Header.Contact.Clear();
            realReq.Header.Contact.Add(contactHeader);

            realReq.Header.Allow = null;
            realReq.Header.From = from;
            realReq.Header.To = to;
            realReq.Header.UserAgent = _msgCore.UserAgent;
            realReq.Header.CSeq = cSeq;
            realReq.Header.CallId = callId;
            realReq.Header.Subject = SetSubject();
            realReq.Header.ContentType = "application/sdp";

            realReq.Body = SetMediaReq(localIp, mediaPort);
            _realReqSession = realReq;
            return realReq;
        }

        private SIPRequest BackVideoPlay(string localIp, int[] mediaPort, int startTime, int endTime, string fromTag, int cSeq, string callId)
        {
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPRequest backReq = new SIPRequest(SIPMethodsEnum.INVITE, remoteUri);
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            backReq.Header.Contact.Clear();
            backReq.Header.Contact.Add(contactHeader);
            backReq.Header.From = from;
            backReq.Header.To = to;
            backReq.Header.CSeq = cSeq;
            backReq.Header.Subject = SetSubject();
            backReq.Header.CallId = callId;
            backReq.Header.ContentType = "application/sdp";

            backReq.Body = SetMediaReq(localIp, mediaPort, startTime, endTime);
            return backReq;
        }

        public void ByeVideoReq()
        {
            if (_realReqSession == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, _realReqSession.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, _realReqSession.Header.To.ToTag);
            SIPRequest byeReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.BYE, remoteUri);
            SIPHeader header = new SIPHeader(from, to, _realReqSession.Header.CSeq, _realReqSession.Header.CallId);
            header.CSeqMethod = byeReq.Header.CSeqMethod;
            header.Vias = byeReq.Header.Vias;
            header.MaxForwards = byeReq.Header.MaxForwards;
            header.UserAgent = _msgCore.UserAgent;
            byeReq.Header.From = from;
            byeReq.Header = header;

            _rtpChannel.Close();
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, byeReq);
            //_byeTask = new TaskTiming(byeReq, _msgCore.Transport);
            //_msgCore.SendRequestTimeout += _byeTask.MessageSendRequestTimeout;
            //_byeTask.Start();
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop()
        {
            if (_realTask != null)
            {
                _realTask.Stop();
            }
            if (_byeTask != null)
            {
                _byeTask.Stop();
            }
        }

        /// <summary>
        /// 设置媒体参数请求(实时)
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">rtp/rtcp媒体端口(10000/10001)</param>
        /// <returns></returns>
        private string SetMediaReq(string localIp, int[] mediaPort)
        {
            SDPConnectionInformation sdpConn = new SDPConnectionInformation(localIp);

            SDP sdp = new SDP();
            sdp.Version = 0;
            sdp.SessionId = "0";
            sdp.Username = _msgCore.LocalSIPId;
            sdp.SessionName = SessionName.Play.ToString();
            sdp.Connection = sdpConn;
            sdp.Timing = "0 0";
            sdp.Address = localIp;

            SDPMediaFormat psFormat = new SDPMediaFormat(SDPMediaFormatsEnum.PS);
            psFormat.IsStandardAttribute = false;
            SDPMediaFormat h264Format = new SDPMediaFormat(SDPMediaFormatsEnum.H264);
            h264Format.IsStandardAttribute = false;
            SDPMediaAnnouncement media = new SDPMediaAnnouncement();

            media.Media = SDPMediaTypesEnum.video;

            media.MediaFormats.Add(psFormat);
            media.MediaFormats.Add(h264Format);
            media.AddExtra("a=recvonly");
            media.AddFormatParameterAttribute(psFormat.FormatID, psFormat.Name);
            media.AddFormatParameterAttribute(h264Format.FormatID, h264Format.Name);
            media.Port = mediaPort[0];

            sdp.Media.Add(media);

            return sdp.ToString();
        }

        /// <summary>
        /// 设置媒体参数请求(回放)
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">rtp/rtcp媒体端口(10000/10001)</param>
        /// <param name="startTime">录像开始时间</param>
        /// <param name="endTime">录像结束数据</param>
        /// <returns></returns>
        private string SetMediaReq(string localIp, int[] mediaPort, int startTime, int endTime)
        {
            SDPConnectionInformation sdpConn = new SDPConnectionInformation(localIp);

            SDP sdp = new SDP();
            sdp.Version = 0;
            sdp.SessionId = "0";
            sdp.Username = _msgCore.LocalSIPId;
            sdp.SessionName = SessionName.Playback.ToString();
            sdp.Connection = sdpConn;
            sdp.Timing = startTime + " " + endTime;
            sdp.Address = localIp;

            SDPMediaFormat psFormat = new SDPMediaFormat(SDPMediaFormatsEnum.PS);
            psFormat.IsStandardAttribute = false;
            SDPMediaFormat h264Format = new SDPMediaFormat(SDPMediaFormatsEnum.H264);
            h264Format.IsStandardAttribute = false;
            SDPMediaAnnouncement media = new SDPMediaAnnouncement();

            media.Media = SDPMediaTypesEnum.video;

            media.MediaFormats.Add(psFormat);
            media.MediaFormats.Add(h264Format);
            media.AddExtra("a=recvonly");
            media.AddFormatParameterAttribute(psFormat.FormatID, psFormat.Name);
            media.AddFormatParameterAttribute(h264Format.FormatID, h264Format.Name);
            media.Port = mediaPort[0];

            sdp.Media.Add(media);

            return sdp.ToString();
        }

        /// <summary>
        /// 设置sip主题
        /// </summary>
        /// <returns></returns>
        private string SetSubject()
        {
            return _deviceId + ":" + 0 + "," + _msgCore.LocalSIPId + ":" + new Random().Next(100, ushort.MaxValue);
        }

        #region 处理PS数据
        private byte[] _publicByte = new byte[0];
        public void Write(byte[] buffer)
        {
            try
            {
                _publicByte = copybyte(_publicByte, buffer);
                int i = 0;
                int BANum = 0;
                if (buffer == null || buffer.Length < 5)
                {
                    return;
                }
                while (i < _publicByte.Length)
                {
                    if (_publicByte[i] == 0x00 && _publicByte[i + 1] == 0x00 && _publicByte[i + 2] == 0x01 && _publicByte[i + 3] == 0xBA)
                    {
                        BANum++;
                        if (BANum == 2)
                        {
                            break;
                        }
                    }
                    i++;
                }

                if (BANum == 2)
                {
                    byte[] psByte = new byte[i];
                    Array.Copy(_publicByte, 0, psByte, 0, i);

                    //处理psByte
                    doPsByte(psByte);

                    byte[] overByte = new byte[_publicByte.Length - i];
                    Array.Copy(_publicByte, i, overByte, 0, overByte.Length);
                    _publicByte = overByte;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public byte[] copybyte(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);
            return c;
        }

        private void doPsByte(byte[] psDate)
        {
            Stream msStream = new System.IO.MemoryStream(psDate);
            List<PESPacket> videoPESList = new List<PESPacket>();

            while (msStream.Length - msStream.Position > 4)
            {
                bool findStartCode = msStream.ReadByte() == 0x00 && msStream.ReadByte() == 0x00 && msStream.ReadByte() == 0x01 && msStream.ReadByte() == 0xE0;
                if (findStartCode)
                {
                    msStream.Seek(-4, SeekOrigin.Current);
                    var pesVideo = new PESPacket();
                    pesVideo.SetBytes(msStream);
                    var esdata = pesVideo.PES_Packet_Data;
                    videoPESList.Add(pesVideo);
                }
            }
            msStream.Close();
            HandlES(videoPESList);
        }

        private void HandlES(List<PESPacket> videoPESList)
        {
            var stream = new MemoryStream();
            foreach (var item in videoPESList)
            {
                stream.Write(item.PES_Packet_Data, 0, item.PES_Packet_Data.Length);
            }
            if (videoPESList.Count == 0)
            {
                stream.Close();
                return;
            }
            var esdata = stream.ToArray();
            stream.Close();
            if (this.m_fs == null)
            {
                this.m_fs = new FileStream("D:\\" + _deviceId + ".h264", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 50 * 1024);
            }
            m_fs.Write(esdata, 0, esdata.Length);
            videoPESList.Clear();
        }
        #endregion
    }
}
