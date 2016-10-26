using log4net;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SIPSorcery.Sys;
using SIPSorcery.Sys.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.Servers.SIPMessage
{
    /// <summary>
    /// SIP服务状态
    /// </summary>
    public enum SipServiceStatus
    {
        /// <summary>
        /// 等待
        /// </summary>
        Wait = 0,
        /// <summary>
        /// 初始化完成
        /// </summary>
        Complete = 1
    }

    /// <summary>
    /// 会话类型
    /// </summary>
    public enum SessionName
    {
        /// <summary>
        /// 实时视频
        /// </summary>
        Play,
        /// <summary>
        /// 录像
        /// </summary>
        Playback
    }

    public class SIPMessageCore
    {
        #region 私有字段
        private static ILog logger = AppState.logger;
        /// <summary>
        /// 本地sip终结点
        /// </summary>
        private SIPEndPoint _localEndPoint;
        /// <summary>
        /// 远程sip终结点
        /// </summary>
        private SIPEndPoint _remoteEndPoint;
        /// <summary>
        /// sip实时视频请求
        /// </summary>
        private SIPRequest _realReqSession;
        /// <summary>
        /// sip传输请求
        /// </summary>
        private SIPTransport _mSIPTransport;
        /// <summary>
        /// 用户代理
        /// </summary>
        private string _userAgent;
        /// <summary>
        /// rtp数据通道
        /// </summary>
        private RTPChannel _rtpChannel;
        /// <summary>
        /// 远程RTCP终结点
        /// </summary>
        private IPEndPoint _rtcpRemoteEndPoint;
        /// <summary>
        /// 媒体端口(0rtp port,1 rtcp port)
        /// </summary>
        private int[] _mediaPort;
        /// <summary>
        /// rtcp套接字连接
        /// </summary>
        private Socket _rtcpSocket;
        /// <summary>
        /// rtcp时间戳
        /// </summary>
        private uint _rtcpTimestamp = 0;
        /// <summary>
        /// rtcp同步源
        /// </summary>
        private uint _rtcpSyncSource = 0;
        private uint _senderPacketCount = 0;
        private uint _senderOctetCount = 0;
        private DateTime _senderLastSentAt = DateTime.MinValue;

        private string _localSIPId;
        private string _remoteSIPId;
        private bool _initSIP = false;
        private int MEDIA_PORT_START = 10000;
        private int MEDIA_PORT_END = 20000;
        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<SipServiceStatus> OnSIPServiceChanged;
        /// <summary>
        /// 设备目录接收
        /// </summary>
        public event Action<Catalog> OnCatalogReceived;
        #endregion

        public SIPMessageCore(SIPTransport transport, string userAgent)
        {
            _mSIPTransport = transport;
            _userAgent = userAgent;
        }

        /// <summary>
        /// sip请求消息
        /// </summary>
        /// <param name="localSIPEndPoint">本地终结点</param>
        /// <param name="remoteEndPoint">远程终结点</param>
        /// <param name="request">sip请求</param>
        public void AddMessageRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest request)
        {
            if (request.Method == SIPMethodsEnum.MESSAGE)
            {
                KeepAlive keepAlive = KeepAlive.Instance.Read(request.Body);
                if (keepAlive != null)  //心跳
                {

                    if (!_initSIP)
                    {
                        _localEndPoint = request.Header.To.ToURI.ToSIPEndPoint();
                        _remoteEndPoint = request.Header.From.FromURI.ToSIPEndPoint();
                        _localSIPId = request.Header.To.ToURI.User;
                        _remoteSIPId = request.Header.From.FromURI.User;
                        _rtpChannel = new RTPChannel(_remoteEndPoint.GetIPEndPoint());
                        _rtpChannel.OnFrameReady += RtpChannel_OnFrameReady;
                    }

                    _initSIP = true;
                    OnSIPServiceChange(SipServiceStatus.Complete);
                }
                else   //目录检索
                {
                    Catalog catalog = Catalog.Instance.Read(request.Body);
                    if (catalog != null)
                    {
                        OnCatalogReceive(catalog);
                    }
                }

                SIPResponse msgRes = GetResponse(localSIPEndPoint, remoteEndPoint, SIPResponseStatusCodesEnum.Ok, "", request);
                _mSIPTransport.SendResponse(msgRes);
            }
            else if (request.Method == SIPMethodsEnum.BYE)
            {
                SIPResponse byeRes = GetResponse(localSIPEndPoint, remoteEndPoint, SIPResponseStatusCodesEnum.Ok, "", request);
                _mSIPTransport.SendResponse(byeRes);

            }
        }

        private void RtpChannel_OnFrameReady(RTPFrame frame)
        {
            
        }

        /// <summary>
        /// sip响应消息
        /// </summary>
        /// <param name="localSIPEndPoint">本地终结点</param>
        /// <param name="remoteEndPoint">远程终结点</param>
        /// <param name="response">sip响应</param>
        public void AddMessageResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse response)
        {
            if (response.Status == SIPResponseStatusCodesEnum.Trying)
            {

            }
            else if (response.Status == SIPResponseStatusCodesEnum.Ok)
            {
                if (_realReqSession.Header.CallId == response.Header.CallId &&
                    response.Header.ContentType.ToLower() == "application/sdp")
                {
                    //SDP okSDP = SDP.ParseSDPDescription(response.Body);
                    SIPRequest ackReq = AckRequest(response);
                    _mSIPTransport.SendRequest(_remoteEndPoint, ackReq);
                }
            }
        }

        public void OnSIPServiceChange(SipServiceStatus state)
        {
            Action<SipServiceStatus> action = OnSIPServiceChanged;

            if (action == null) return;

            foreach (Action<SipServiceStatus> handler in action.GetInvocationList())
            {
                try { handler(state); }
                catch { continue; }
            }
        }

        public void OnCatalogReceive(Catalog cata)
        {
            Action<Catalog> action = OnCatalogReceived;
            if (action == null) return;

            foreach (Action<Catalog> handler in action.GetInvocationList())
            {
                try { handler(cata); }
                catch { continue; }
            }
        }

        private SIPResponse GetResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponseStatusCodesEnum responseCode, string reasonPhrase, SIPRequest request)
        {
            try
            {
                SIPResponse response = new SIPResponse(responseCode, reasonPhrase, localSIPEndPoint);
                SIPSchemesEnum sipScheme = (localSIPEndPoint.Protocol == SIPProtocolsEnum.tls) ? SIPSchemesEnum.sips : SIPSchemesEnum.sip;
                SIPFromHeader from = request.Header.From;
                from.FromTag = request.Header.From.FromTag;
                SIPToHeader to = request.Header.To;
                response.Header = new SIPHeader(from, to, request.Header.CSeq, request.Header.CallId);
                response.Header.CSeqMethod = request.Header.CSeqMethod;
                response.Header.Vias = request.Header.Vias;
                //response.Header.Server = _userAgent;
                response.Header.UserAgent = _userAgent;
                response.Header.CSeq = request.Header.CSeq;

                if (response.Header.To.ToTag == null || request.Header.To.ToTag.Trim().Length == 0)
                {
                    response.Header.To.ToTag = CallProperties.CreateNewTag();
                }

                return response;
            }
            catch (Exception excp)
            {
                logger.Error("Exception SIPTransport GetResponse. " + excp.Message);
                throw;
            }
        }

        /// <summary>
        /// 实时视频请求
        /// </summary>
        /// <param name="deviceId">设备编码</param>
        public void RealVideoReq(string deviceId)
        {
            if (_localEndPoint == null)
            {
                OnSIPServiceChange(SipServiceStatus.Wait);
                return;
            }

            _mediaPort = SetMediaPort();
            string localIp = _localEndPoint.Address.ToString();
            SDPConnectionInformation sdpConn = new SDPConnectionInformation(localIp);

            SDP sdp = new SDP();
            sdp.Version = 0;
            sdp.SessionId = "0";
            sdp.Username = _localSIPId;
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
            media.Port = _mediaPort[0];

            sdp.Media.Add(media);
            string str = sdp.ToString();

            SIPURI remoteUri = new SIPURI(_remoteSIPId, _remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_localSIPId, _localEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest catalogReq = _mSIPTransport.GetRequest(SIPMethodsEnum.INVITE, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);

            catalogReq.Header.From = from;
            catalogReq.Header.Contact.Clear();
            catalogReq.Header.Contact.Add(contactHeader);
            catalogReq.Header.Allow = null;
            catalogReq.Header.To = to;
            catalogReq.Header.UserAgent = _userAgent;
            catalogReq.Header.CSeq = CallProperties.CreateNewCSeq();
            catalogReq.Header.CallId = CallProperties.CreateNewCallId();
            catalogReq.Header.Subject = ToSubject();
            catalogReq.Header.ContentType = "Application/sdp";

            catalogReq.Body = sdp.ToString();
            _realReqSession = catalogReq;
            _mSIPTransport.SendRequest(_remoteEndPoint, catalogReq);


            _rtpChannel.IsClosed = false;
            _rtpChannel.ReservePorts(_mediaPort[0], _mediaPort[1]);
            _rtpChannel.Start();
        }

        /// <summary>
        /// 确认接收视频请求
        /// </summary>
        /// <param name="response">响应消息</param>
        /// <returns></returns>
        private SIPRequest AckRequest(SIPResponse response)
        {
            SIPURI localUri = new SIPURI(_localSIPId, _localEndPoint.ToHost(), "");
            SIPURI remoteURI = new SIPURI(_remoteSIPId, _remoteEndPoint.ToHost(), "");
            SIPRequest ackRequest = _mSIPTransport.GetRequest(SIPMethodsEnum.ACK, remoteURI);
            SIPFromHeader from = new SIPFromHeader(null, _realReqSession.Header.From.FromURI, response.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteURI, response.Header.To.ToTag);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            SIPHeader header = new SIPHeader(from, to, response.Header.CSeq, response.Header.CallId);
            header.CSeqMethod = SIPMethodsEnum.ACK;
            header.Contact = response.Header.Contact;
            header.Contact.Clear();
            header.Contact.Add(contactHeader);
            header.Vias = response.Header.Vias;
            header.MaxForwards = response.Header.MaxForwards;
            header.ContentLength = response.Header.ContentLength;
            header.UserAgent = _userAgent;
            header.Allow = null;
            ackRequest.Header = header;
            return ackRequest;
        }

        public void ByeVideoReq(string p)
        {
            if (_localEndPoint == null)
            {
                OnSIPServiceChange(SipServiceStatus.Wait);
                return;
            }
            SIPURI localURI = new SIPURI(_localSIPId, _localEndPoint.ToHost(), "");
            SIPURI remoteURI = new SIPURI(_remoteSIPId, _remoteEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localURI, _realReqSession.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteURI, _realReqSession.Header.To.ToTag);
            SIPRequest byeReqr = _mSIPTransport.GetRequest(SIPMethodsEnum.BYE, remoteURI);
            SIPHeader header = new SIPHeader(from, to, _realReqSession.Header.CSeq, _realReqSession.Header.CallId);
            header.CSeqMethod = byeReqr.Header.CSeqMethod;
            header.Vias = byeReqr.Header.Vias;
            header.MaxForwards = byeReqr.Header.MaxForwards;
            header.UserAgent = _userAgent;
            byeReqr.Header.From = from;
            byeReqr.Header = header;
            _mSIPTransport.SendRequest(_remoteEndPoint, byeReqr);
        }

        private string ToSubject()
        {
            return _remoteSIPId + ":" + 0 + "," + _localSIPId + ":" + new Random().Next(ushort.MaxValue);
        }

        /// <summary>
        /// 设备目录查询
        /// </summary>
        /// <param name="cameraId">目的设备编码</param>
        public void DeviceCatalogQuery(string deviceId)
        {
            if (_localEndPoint == null)
            {
                OnSIPServiceChange(SipServiceStatus.Wait);
                return;
            }
            SIPRequest req = QueryItems();
            CatalogQuery catalog = new CatalogQuery()
            {
                CommandType = VariableType.Catalog,
                DeviceID = deviceId,
                SN = new Random().Next(9999)
            };
            string xmlBody = CatalogQuery.Instance.Save<CatalogQuery>(catalog);
            req.Body = xmlBody;
            _mSIPTransport.SendRequest(_remoteEndPoint, req);
        }

        /// <summary>
        /// 查询设备目录请求
        /// </summary>
        /// <returns></returns>
        private SIPRequest QueryItems()
        {
            SIPURI remoteUri = new SIPURI(_remoteSIPId, _remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_localSIPId, _localEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
            SIPToHeader to = new SIPToHeader(null, remoteUri, CallProperties.CreateNewTag());
            SIPRequest catalogReq = _mSIPTransport.GetRequest(SIPMethodsEnum.MESSAGE, remoteUri);
            catalogReq.Header.From = from;
            catalogReq.Header.Contact = null;
            catalogReq.Header.Allow = null;
            catalogReq.Header.To = to;
            catalogReq.Header.UserAgent = _userAgent;
            catalogReq.Header.CSeq = CallProperties.CreateNewCSeq();
            catalogReq.Header.CallId = CallProperties.CreateNewCallId();
            catalogReq.Header.ContentType = "Application/MANSCDP+xml";
            return catalogReq;
        }



        /// <summary>
        /// 设置媒体(rtp/rtcp)端口号
        /// </summary>
        /// <returns></returns>
        public int[] SetMediaPort()
        {
            var inUseUDPPorts = (from p in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port >= MEDIA_PORT_START select p.Port).OrderBy(x => x).ToList();

            int rtpPort = 0;
            int controlPort = 0;

            if (inUseUDPPorts.Count > 0)
            {
                // Find the first two available for the RTP socket.
                for (int index = MEDIA_PORT_START; index <= MEDIA_PORT_END; index++)
                {
                    if (!inUseUDPPorts.Contains(index))
                    {
                        rtpPort = index;
                        break;
                    }
                }

                // Find the next available for the control socket.
                for (int index = rtpPort + 1; index <= MEDIA_PORT_END; index++)
                {
                    if (!inUseUDPPorts.Contains(index))
                    {
                        controlPort = index;
                        break;
                    }
                }
            }
            else
            {
                rtpPort = MEDIA_PORT_START;
                controlPort = MEDIA_PORT_START + 1;
            }

            if (MEDIA_PORT_START >= MEDIA_PORT_END)
            {
                MEDIA_PORT_START = 10000;
            }
            MEDIA_PORT_START += 2;
            int[] mediaPort = new int[2];
            mediaPort[0] = rtpPort;
            mediaPort[1] = controlPort;
            return mediaPort;
        }


    }

    //    #region 构造函数
    //    /// <summary>
    //    /// sip监控初始化
    //    /// </summary>
    //    /// <param name="messageCore">sip消息</param>
    //    /// <param name="sipTransport">sip传输</param>
    //    /// <param name="cameraId">摄像机编码</param>
    //    public SIPMessageCore(SIPMessageCore messageCore, string cameraId)
    //    {
    //        _messageCore = messageCore;
    //        _m_sipTransport = messageCore.m_sipTransport;
    //        _cameraId = cameraId;
    //        _userAgent = messageCore.m_userAgent;
    //        _rtcpSyncSource = Convert.ToUInt32(Crypto.GetRandomInt(0, 9999999));

    //        _messageCore.SipRequestInited += messageCore_SipRequestInited;
    //        _messageCore.SipInviteVideoOK += messageCore_SipInviteVideoOK;
    //    } 
    //    #endregion

    //    #region 确认视频请求
    //    /// <summary>
    //    /// 实时视频请求成功事件处理
    //    /// </summary>
    //    /// <param name="res"></param>
    //    private void messageCore_SipInviteVideoOK(SIPResponse res)
    //    {
    //        if (_realReqSession == null)
    //        {
    //            return;
    //        }
    //        //同一会话消息
    //        if (_realReqSession.Header.CallId == res.Header.CallId)
    //        {
    //            RealVideoRes realRes = RealVideoRes.Instance.Read(res.Body);
    //            GetRemoteRtcp(realRes.Socket);

    //            SIPRequest ackReq = AckRequest(res);
    //            _m_sipTransport.SendRequest(_remoteEndPoint, ackReq);
    //        }
    //    } 
    //    #endregion

    //    #region rtp/rtcp事件处理
    //    /// <summary>
    //    /// sip初始化完成事件
    //    /// </summary>
    //    /// <param name="sipRequest">sip请求</param>
    //    /// <param name="localEndPoint">本地终结点</param>
    //    /// <param name="remoteEndPoint">远程终结点</param>
    //    /// <param name="sipAccount">sip账户</param>
    //    private void messageCore_SipRequestInited(SIPRequest sipRequest, SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPAccount sipAccount)
    //    {
    //        _sipInited = true;
    //        _sipRequest = sipRequest;
    //        _localEndPoint = localEndPoint;
    //        _remoteEndPoint = remoteEndPoint;
    //        _sipAccount = sipAccount;

    //        _rtpRemoteEndPoint = new IPEndPoint(remoteEndPoint.Address, remoteEndPoint.Port);
    //        _rtpChannel = new RTPChannel(_rtpRemoteEndPoint);
    //        _rtpChannel.OnFrameReady += _rtpChannel_OnFrameReady;
    //        _rtpChannel.OnControlDataReceived += _rtpChannel_OnControlDataReceived;

    //        if (SipStatusHandler != null)
    //        {
    //            SipStatusHandler(SipServiceStatus.Inited);
    //        }
    //        _messageCore.SipRequestInited -= messageCore_SipRequestInited;
    //    }

    //    /// <summary>
    //    /// rtp包回调事件处理
    //    /// </summary>
    //    /// <param name="frame"></param>
    //    private void _rtpChannel_OnFrameReady(RTPFrame frame)
    //    {
    //        //byte[] buffer = frame.GetFramePayload();
    //        //Write(buffer);
    //    }

    //    /// <summary>
    //    /// rtcp包回调事件处理
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    /// <param name="rtcpSocket"></param>
    //    private void _rtpChannel_OnControlDataReceived(byte[] buffer, Socket rtcpSocket)
    //    {
    //        _rtcpSocket = rtcpSocket;
    //        DateTime packetTimestamp = DateTime.Now;
    //        _rtcpTimestamp = RTPChannel.DateTimeToNptTimestamp90K(DateTime.Now);
    //        if (_rtcpRemoteEndPoint != null)
    //        {
    //            SendRtcpSenderReport(RTPChannel.DateTimeToNptTimestamp(packetTimestamp), _rtcpTimestamp);
    //        }
    //    }

    //    /// <summary>
    //    /// 发送rtcp包
    //    /// </summary>
    //    /// <param name="ntpTimestamp"></param>
    //    /// <param name="rtpTimestamp"></param>
    //    private void SendRtcpSenderReport(ulong ntpTimestamp, uint rtpTimestamp)
    //    {
    //        try
    //        {
    //            RTCPPacket senderReport = new RTCPPacket(_rtcpSyncSource, ntpTimestamp, rtpTimestamp, _senderPacketCount, _senderOctetCount);
    //            var bytes = senderReport.GetBytes();
    //            _rtcpSocket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, _rtcpRemoteEndPoint, SendRtcpCallback, _rtcpSocket);
    //            _senderLastSentAt = DateTime.Now;
    //        }
    //        catch (Exception excp)
    //        {
    //            logger.Error("Exception SendRtcpSenderReport. " + excp);
    //        }
    //    }

    //    /// <summary>
    //    /// 发送rtcp回调
    //    /// </summary>
    //    /// <param name="ar"></param>
    //    private void SendRtcpCallback(IAsyncResult ar)
    //    {
    //        try
    //        {
    //            _rtcpSocket.EndSend(ar);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Warn("Exception Rtcp", ex);
    //        }
    //    } 
    //    #endregion

    //    #region sip视频请求
    //    /// <summary>
    //    /// 实时视频请求
    //    /// </summary>
    //    public void RealVideoRequest()
    //    {
    //        if (!_sipInited)
    //        {
    //            if (SipStatusHandler != null)
    //            {
    //                SipStatusHandler(SipServiceStatus.Wait);
    //            }
    //            return;
    //        }
    //        _mediaPort = _messageCore.SetMediaPort();

    //        SIPRequest request = InviteRequest();
    //        RealVideo real = new RealVideo()
    //        {
    //            Address = _cameraId,
    //            Variable = VariableType.RealMedia,
    //            Privilege = 90,
    //            Format = "4CIF CIF QCIF 720p 1080p",
    //            Video = "H.264",
    //            Audio = "G.711",
    //            MaxBitrate = 800,
    //            Socket = this.ToString()
    //        };

    //        string xmlBody = RealVideo.Instance.Save<RealVideo>(real);
    //        request.Body = xmlBody;
    //        _m_sipTransport.SendRequest(_remoteEndPoint, request);

    //        //启动RTP通道
    //        _rtpChannel.IsClosed = false;
    //        _rtpChannel.ReservePorts(_mediaPort[0], _mediaPort[1]);
    //        _rtpChannel.Start();
    //    }

    //    /// <summary>
    //    /// 实时视频取消
    //    /// </summary>
    //    public void RealVideoBye()
    //    {
    //        if (!_sipInited)
    //        {
    //            if (SipStatusHandler != null)
    //            {
    //                SipStatusHandler(SipServiceStatus.Wait);
    //            }
    //            return;
    //        }
    //        _rtpChannel.Close();
    //        if (_realReqSession == null)
    //        {
    //            return;
    //        }
    //        SIPRequest req = ByeRequest();
    //        _m_sipTransport.SendRequest(_remoteEndPoint, req);
    //    }

    //    /// <summary>
    //    /// 查询前端设备信息
    //    /// </summary>
    //    /// <param name="cameraId"></param>
    //    public void DeviceQuery(string cameraId)
    //    {
    //        if (!_sipInited)
    //        {
    //            if (SipStatusHandler != null)
    //            {
    //                SipStatusHandler(SipServiceStatus.Wait);
    //            }
    //            return;
    //        }
    //        Device dev = new Device()
    //        {
    //            Privilege = 90,
    //            Variable = VariableType.DeviceInfo
    //        };
    //        SIPRequest req = DeviceReq(cameraId);
    //        string xmlBody = Device.Instance.Save<Device>(dev);
    //        req.Body = xmlBody;
    //        _m_sipTransport.SendRequest(_remoteEndPoint, req);
    //    }



    //    private SIPRequest ByeRequest()
    //    {
    //        SIPURI uri = new SIPURI(_cameraId, _remoteEndPoint.ToHost(), "");
    //        SIPRequest byeRequest = _m_sipTransport.GetRequest(SIPMethodsEnum.BYE, uri);
    //        SIPFromHeader from = new SIPFromHeader(null, _sipRequest.URI, _realReqSession.Header.From.FromTag);
    //        SIPHeader header = new SIPHeader(from, byeRequest.Header.To, _realReqSession.Header.CSeq, _realReqSession.Header.CallId);
    //        header.ContentType = "application/DDCP";
    //        header.Expires = byeRequest.Header.Expires;
    //        header.CSeqMethod = byeRequest.Header.CSeqMethod;
    //        header.Vias = byeRequest.Header.Vias;
    //        header.MaxForwards = byeRequest.Header.MaxForwards;
    //        header.UserAgent = _userAgent;
    //        byeRequest.Header.From = from;
    //        byeRequest.Header = header;
    //        return byeRequest;
    //    }

    //    /// <summary>
    //    /// 前端设备信息请求
    //    /// </summary>
    //    /// <param name="cameraId"></param>
    //    /// <returns></returns>
    //    private SIPRequest DeviceReq(string cameraId)
    //    {
    //        SIPURI remoteUri = new SIPURI(cameraId, _remoteEndPoint.ToHost(), "");
    //        SIPURI localUri = new SIPURI(_sipAccount.LocalSipId, _localEndPoint.ToHost(), "");
    //        SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
    //        SIPToHeader to = new SIPToHeader(null, remoteUri, null);
    //        SIPRequest queryReq = _m_sipTransport.GetRequest(SIPMethodsEnum.DO, remoteUri);
    //        queryReq.Header.Contact = null;
    //        queryReq.Header.From = from;
    //        queryReq.Header.Allow = null;
    //        queryReq.Header.To = to;
    //        queryReq.Header.CSeq = CallProperties.CreateNewCSeq();
    //        queryReq.Header.CallId = CallProperties.CreateNewCallId();
    //        queryReq.Header.ContentType = "Application/DDCP";
    //        return queryReq;
    //    }

    //    /// <summary>
    //    /// 查询设备目录请求
    //    /// </summary>
    //    /// <returns></returns>
    //    private SIPRequest QueryItems()
    //    {
    //        SIPURI remoteUri = new SIPURI(_sipAccount.RemoteSipId, _remoteEndPoint.ToHost(), "");
    //        SIPURI localUri = new SIPURI(_sipAccount.LocalSipId, _localEndPoint.ToHost(), "");
    //        SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
    //        SIPToHeader to = new SIPToHeader(null, remoteUri, null);
    //        SIPRequest queryReq = _m_sipTransport.GetRequest(SIPMethodsEnum.DO, remoteUri);
    //        queryReq.Header.From = from;
    //        queryReq.Header.Contact = null;
    //        queryReq.Header.Allow = null;
    //        queryReq.Header.To = to;
    //        queryReq.Header.CSeq = CallProperties.CreateNewCSeq();
    //        queryReq.Header.CallId = CallProperties.CreateNewCallId();
    //        queryReq.Header.ContentType = "Application/DDCP";
    //        return queryReq;
    //    }

    //    /// <summary>
    //    /// 监控视频请求
    //    /// </summary>
    //    /// <returns></returns>
    //    private SIPRequest InviteRequest()
    //    {
    //        SIPURI uri = new SIPURI(_cameraId, _remoteEndPoint.ToHost(), "");
    //        SIPRequest inviteRequest = _m_sipTransport.GetRequest(SIPMethodsEnum.INVITE, uri);
    //        SIPFromHeader from = new SIPFromHeader(null, _sipRequest.URI, CallProperties.CreateNewTag());
    //        SIPHeader header = new SIPHeader(from, inviteRequest.Header.To, CallProperties.CreateNewCSeq(), CallProperties.CreateNewCallId());
    //        header.ContentType = "application/DDCP";
    //        header.Expires = inviteRequest.Header.Expires;
    //        header.CSeqMethod = inviteRequest.Header.CSeqMethod;
    //        header.Vias = inviteRequest.Header.Vias;
    //        header.MaxForwards = inviteRequest.Header.MaxForwards;
    //        header.UserAgent = _userAgent;
    //        inviteRequest.Header.From = from;
    //        inviteRequest.Header = header;
    //        _realReqSession = inviteRequest;
    //        return inviteRequest;
    //    }

    //    /// <summary>
    //    /// 确认接收视频请求
    //    /// </summary>
    //    /// <param name="response">响应消息</param>
    //    /// <returns></returns>
    //    private SIPRequest AckRequest(SIPResponse response)
    //    {
    //        SIPURI uri = new SIPURI(response.Header.To.ToURI.User, _remoteEndPoint.ToHost(), "");
    //        SIPRequest ackRequest = _m_sipTransport.GetRequest(SIPMethodsEnum.ACK, uri);
    //        SIPFromHeader from = new SIPFromHeader(null, _sipRequest.URI, response.Header.CallId);
    //        from.FromTag = response.Header.From.FromTag;
    //        SIPHeader header = new SIPHeader(from, response.Header.To, response.Header.CSeq, response.Header.CallId);
    //        header.To.ToTag = null;
    //        header.CSeqMethod = SIPMethodsEnum.ACK;
    //        header.Vias = response.Header.Vias;
    //        header.MaxForwards = response.Header.MaxForwards;
    //        header.ContentLength = response.Header.ContentLength;
    //        header.UserAgent = _userAgent;
    //        header.Allow = null;
    //        ackRequest.Header = header;
    //        return ackRequest;
    //    } 
    //    #endregion

    //    #region 私有方法

    //    /// <summary>
    //    /// 获取远程rtcp终结点(192.168.10.250 UDP 5000)
    //    /// </summary>
    //    /// <param name="socket"></param>
    //    private void GetRemoteRtcp(string socket)
    //    {
    //        string[] split = socket.Split(' ');
    //        if (split.Length < 3)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            IPAddress remoteIP = _remoteEndPoint.Address;
    //            IPAddress.TryParse(split[0], out remoteIP);
    //            int rtcpPort = _mediaPort[1];
    //            int.TryParse(split[2], out rtcpPort);
    //            _rtcpRemoteEndPoint = new IPEndPoint(remoteIP, rtcpPort + 1);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Warn("remote rtp ip/port error", ex);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return _localEndPoint.Address.ToString() + " UDP " + _mediaPort[0];
    //    } 
    //    #endregion
    //}
}
