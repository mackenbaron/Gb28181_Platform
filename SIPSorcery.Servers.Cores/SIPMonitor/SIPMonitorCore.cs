using SIPSorcery.Net;
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

        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<string, SipServiceStatus> OnSIPServiceChanged;

        public SIPMonitorCore(SIPMessageCore msgCore, string deviceId, string name)
        {
            _msgCore = msgCore;
            _deviceId = deviceId;
            _deviceName = name;
            _rtpChannel = new RTPChannel();
            _rtpChannel.OnFrameReady += _rtpChannel_OnFrameReady;
        }

        //FileStream m_fs = null;
        private void _rtpChannel_OnFrameReady(RTPFrame frame)
        {
            //if (this.m_fs == null)
            //{
            //    this.m_fs = new FileStream("D:\\test.h264", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 50 * 1024);
            //}
            //byte[] buffer = frame.GetFramePayload();
            //this.m_fs.Write(buffer, 0, buffer.Length);
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

            _rtpChannel.RemoteEndPoint = _msgCore.RemoteEndPoint.GetIPEndPoint();
            int[] mediaPort = _msgCore.SetMediaPort();

            string localIp = _msgCore.LocalEndPoint.Address.ToString();
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

            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest realReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INVITE, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);

            realReq.Header.From = from;
            realReq.Header.Contact.Clear();
            realReq.Header.Contact.Add(contactHeader);
            realReq.Header.Allow = null;
            realReq.Header.To = to;
            realReq.Header.UserAgent = _msgCore.UserAgent;
            realReq.Header.CSeq = CallProperties.CreateNewCSeq();
            realReq.Header.CallId = CallProperties.CreateNewCallId();
            realReq.Header.Subject = ToSubject();
            realReq.Header.ContentType = "Application/sdp";

            realReq.Body = sdp.ToString();
            _msgCore.RealReqSession = realReq;
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, realReq);

            _rtpChannel.IsClosed = false;
            _rtpChannel.ReservePorts(mediaPort[0], mediaPort[1]);
            _rtpChannel.SetFrameType(FrameTypesEnum.H264);
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
            SIPFromHeader from = new SIPFromHeader(null, _msgCore.RealReqSession.Header.From.FromURI, response.Header.From.FromTag);
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

        public void ByeVideoReq()
        {
            if (_msgCore.RealReqSession == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }
            _rtpChannel.Close();
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, _msgCore.RealReqSession.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, _msgCore.RealReqSession.Header.To.ToTag);
            SIPRequest byeReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.BYE, remoteUri);
            SIPHeader header = new SIPHeader(from, to, _msgCore.RealReqSession.Header.CSeq, _msgCore.RealReqSession.Header.CallId);
            header.CSeqMethod = byeReq.Header.CSeqMethod;
            header.Vias = byeReq.Header.Vias;
            header.MaxForwards = byeReq.Header.MaxForwards;
            header.UserAgent = _msgCore.UserAgent;
            byeReq.Header.From = from;
            byeReq.Header = header;
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, byeReq);
        }

        private string ToSubject()
        {
            return _msgCore.RemoteSIPId + ":" + 0 + "," + _msgCore.LocalSIPId + ":" + new Random().Next(ushort.MaxValue);
        }
    }
}
