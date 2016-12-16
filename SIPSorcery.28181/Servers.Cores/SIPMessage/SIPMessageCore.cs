using log4net;
using SIPSorcery.GB28181.Net;
using SIPSorcery.GB28181.Servers.SIPMonitor;
using SIPSorcery.GB28181.SIP;
using SIPSorcery.GB28181.SIP.App;
using SIPSorcery.GB28181.Sys;
using SIPSorcery.GB28181.Sys.Config;
using SIPSorcery.GB28181.Sys.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SIPSorcery.GB28181.Servers.SIPMessage
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
    /// sip消息核心处理
    /// </summary>
    public class SIPMessageCore
    {
        #region 私有字段

        private static ILog logger = AppState.logger;

        private bool _initSIP;
        private int MEDIA_PORT_START = 30000;
        private int MEDIA_PORT_END = 32000;
        private RegistrarCore m_registrarCore;
        private TaskTiming _catalogTask;

        /// <summary>
        /// 用户代理
        /// </summary>
        internal string UserAgent = SIPConstants.SIP_USERAGENT_STRING;
        /// <summary>
        /// 本地sip终结点
        /// </summary>
        internal SIPEndPoint LocalEndPoint;
        /// <summary>
        /// 远程sip终结点
        /// </summary>
        internal SIPEndPoint RemoteEndPoint;
        /// <summary>
        /// sip传输请求
        /// </summary>
        internal SIPTransport Transport;
        /// <summary>
        /// 远程sip传输集合
        /// </summary>
        internal Dictionary<string, string> RemoteTrans;
        /// <summary>
        /// 本地sip编码
        /// </summary>
        internal string LocalSIPId;
        /// <summary>
        /// 远程sip编码
        /// </summary>
        internal string RemoteSIPId;
        /// <summary>
        /// 监控服务
        /// </summary>
        public Dictionary<string, ISIPMonitorService> MonitorService;
        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<string, SipServiceStatus> OnSIPServiceChanged;
        /// <summary>
        /// 设备目录接收
        /// </summary>
        public event Action<Catalog> OnCatalogReceived;
        /// <summary>
        /// 录像文件接收
        /// </summary>
        public event Action<RecordInfo> OnRecordInfoReceived;
        /// <summary>
        /// 消息发送超时
        /// </summary>
        public event Action<SIPResponse> SendRequestTimeout;
        #endregion

        public SIPMessageCore(SIPTransport transport, string userAgent)
        {
            Transport = transport;
            RemoteTrans = new Dictionary<string, string>();

        }

        public void Initialize(SIPAuthenticateRequestDelegate sipRequestAuthenticator,
            Dictionary<string, PlatformConfig> platformList,
            SIPAccount account)
        {
            LocalEndPoint = SIPEndPoint.ParseSIPEndPoint("udp:" + account.LocalIP.ToString() + ":" + account.LocalPort);
            LocalSIPId = account.LocalID;
            //foreach (var account in accounts)
            //{
            //    SIPEndPoint remoteEP = SIPEndPoint.ParseSIPEndPoint("udp:" + account.SIPDomain);
            //    RemoteTrans.Add(remoteEP.ToString(), account.SIPUsername);
            //    if (LocalEndPoint == null)
            //    {
            //        LocalEndPoint = SIPEndPoint.ParseSIPEndPoint("udp:" + account.LocalIP.ToString() + ":" + account.LocalPort);
            //    }
            //    if (LocalSIPId == null)
            //    {
            //        LocalSIPId = account.LocalID;
            //    }
            //}

            m_registrarCore = new RegistrarCore(Transport, true, true, sipRequestAuthenticator);
            m_registrarCore.Start(1);
            MonitorService = new Dictionary<string, ISIPMonitorService>();

            foreach (var item in platformList)
            {
                string sipEndPointStr = "udp:" + item.Value.RemoteIP + ":" + item.Value.RemotePort;
                SIPEndPoint sipPoint = SIPEndPoint.ParseSIPEndPoint(sipEndPointStr);
                for (int i = 0; i < 2; i++)
                {
                    CommandType cmdType = CommandType.Unknown;
                    if (i == 0)
                    {
                        cmdType = CommandType.Play;
                    }
                    else
                    {
                        cmdType = CommandType.Playback;
                    }
                    string key = item.Key + cmdType;
                    ISIPMonitorService monitor = new SIPMonitorCore(this, item.Key, item.Value.ChannelName, sipPoint);
                    monitor.OnSIPServiceChanged += monitor_OnSIPServiceChanged;
                    MonitorService.Add(key, monitor);
                }
            }
        }

        /// <summary>
        /// 初始化远程sip
        /// </summary>
        /// <param name="localEndPoint">本地终结点</param>
        /// <param name="remoteEndPoint">远程终结点</param>
        /// <param name="request">sip请求</param>
        private void SIPTransportInit(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest request)
        {
            lock (RemoteTrans)
            {
                if (!RemoteTrans.ContainsKey(remoteEndPoint.ToString()))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    RemoteTrans.Add(remoteEndPoint.ToString(), request.Header.From.FromURI.User);
                    logger.Debug("RemoteTrans Init:Remote:" + remoteEndPoint.ToHost() + "-----User:" + request.Header.From.FromURI.User);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        /// <summary>
        /// sip请求消息
        /// </summary>
        /// <param name="localEndPoint">本地终结点</param>
        /// <param name="remoteEndPoint"b>远程终结点</param>
        /// <param name="request">sip请求</param>
        public void AddMessageRequest(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest request)
        {
            //注册请求
            if (request.Method == SIPMethodsEnum.REGISTER)
            {
                m_registrarCore.AddRegisterRequest(localEndPoint, remoteEndPoint, request);
                SIPTransportInit(localEndPoint, remoteEndPoint, request);
            }
            //消息请求
            else if (request.Method == SIPMethodsEnum.MESSAGE)
            {
                SIPTransportInit(localEndPoint, remoteEndPoint, request);
                KeepAlive keepAlive = KeepAlive.Instance.Read(request.Body);
                if (keepAlive != null && keepAlive.CmdType == CommandType.Keepalive)  //心跳
                {
                    //if (!_initSIP)
                    //{
                    //LocalEndPoint = request.Header.To.ToURI.ToSIPEndPoint();
                    //RemoteEndPoint = request.Header.From.FromURI.ToSIPEndPoint();
                    //LocalSIPId = request.Header.To.ToURI.User;
                    //RemoteSIPId = request.Header.From.FromURI.User;
                    //}

                    //_initSIP = true;
                    logger.Debug("KeepAlive:" + remoteEndPoint.ToHost() + "=====DevID:" + keepAlive.DeviceID + "=====Status:" + keepAlive.Status + "=====SN:" + keepAlive.SN);
                    OnSIPServiceChange(remoteEndPoint.ToHost(), SipServiceStatus.Complete);
                }
                else
                {
                    Catalog catalog = Catalog.Instance.Read(request.Body);
                    if (catalog != null && catalog.CmdType == CommandType.Catalog)  //设备目录
                    {
                        foreach (var cata in catalog.DeviceList.Items)
                        {
                            cata.RemoteEP = request.Header.From.FromURI.Host;
                            for (int i = 0; i < 2; i++)
                            {
                                CommandType cmdType = CommandType.Unknown;
                                if (i == 0)
                                {
                                    cmdType = CommandType.Play;
                                }
                                else
                                {
                                    cmdType = CommandType.Playback;
                                }
                                string key = cata.DeviceID + cmdType;
                                lock (MonitorService)
                                {
                                    if (MonitorService.ContainsKey(key))
                                    {
                                        continue;
                                    }
                                    ISIPMonitorService monitor = new SIPMonitorCore(this, cata.DeviceID, cata.Name, remoteEndPoint);
                                    monitor.OnSIPServiceChanged += monitor_OnSIPServiceChanged;
                                    MonitorService.Add(key, monitor);
                                }
                            }
                        }
                        if (OnCatalogReceived != null)
                        {
                            OnCatalogReceived(catalog);
                        }
                    }
                    RecordInfo record = RecordInfo.Instance.Read(request.Body);
                    if (record != null && record.CmdType == CommandType.RecordInfo)  //录像检索
                    {
                        lock (MonitorService)
                        {
                            MonitorService[record.DeviceID + CommandType.Playback].RecordQueryTotal(record.SumNum);
                        }
                        if (OnRecordInfoReceived != null && record.RecordItems != null)
                        {
                            OnRecordInfoReceived(record);
                        }
                    }
                }
                SIPResponse msgRes = GetResponse(localEndPoint, remoteEndPoint, SIPResponseStatusCodesEnum.Ok, "", request);
                Transport.SendResponse(msgRes);
            }
            //停止播放请求
            else if (request.Method == SIPMethodsEnum.BYE)
            {
                SIPResponse byeRes = GetResponse(localEndPoint, remoteEndPoint, SIPResponseStatusCodesEnum.Ok, "", request);
                Transport.SendResponse(byeRes);
            }
        }

        /// <summary>
        /// sip响应消息
        /// </summary>
        /// <param name="localSIPEndPoint">本地终结点</param>
        /// <param name="remoteEndPoint">远程终结点</param>
        /// <param name="response">sip响应</param>
        public void AddMessageResponse(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse response)
        {
            if (SendRequestTimeout != null)
            {
                SendRequestTimeout(response);
            }
            if (response.Status == SIPResponseStatusCodesEnum.Trying)
            {
                logger.Debug("up platform return waiting process msg | " + response.Status);
            }
            else if (response.Status == SIPResponseStatusCodesEnum.Ok)
            {
                if (response.Header.ContentType.ToLower() == "application/sdp")
                {
                    //CommandType cmdType = CommandType.Unknown;
                    //logger.Debug(response.Body);
                    //SDP sdp = SDP.ParseSDPDescription(response.Body);

                    //if (sdp != null)
                    //{
                    //    Enum.TryParse<CommandType>(sdp.SessionName, out cmdType);
                    //}

                    CommandType cmdType = CommandType.Play;
                    string sessionName = GetSessionName(response.Body);
                    if (sessionName != null)
                    {
                        Enum.TryParse<CommandType>(sessionName, out cmdType);
                    }
                    string key = response.Header.To.ToURI.User + cmdType;
                    MonitorService[key].AckRequest(response);
                }
            }
            else if (response.Status == SIPResponseStatusCodesEnum.BadRequest)  //请求失败
            {
                string toUser = response.Header.To.ToURI.User;
                string msg = toUser + "===RealVideo 400 " + response.Status;
                logger.Debug(msg);
                if (response.Header.Warning != null)
                {
                    msg += response.Header.Warning;
                }
                BadRequest(msg, toUser, response.Header.CallId);
            }
            else if (response.Status == SIPResponseStatusCodesEnum.InternalServerError) //服务器内部错误
            {
                string toUser = response.Header.To.ToURI.User;
                string msg = toUser + "===RealVideo 500 " + response.Status;
                logger.Debug(msg);
                if (response.Header.ErrorInfo != null)
                {
                    msg += response.Header.ErrorInfo;
                }
                BadRequest(msg, toUser, response.Header.CallId);
            }
            else if (response.Status == SIPResponseStatusCodesEnum.RequestTerminated)   //请求终止
            {
                string toUser = response.Header.To.ToURI.User;
                string msg = toUser + "===RealVideo 487 " + response.Status;
                logger.Debug(msg);
                if (response.Header.ErrorInfo != null)
                {
                    msg += response.Header.ErrorInfo;
                }
                BadRequest(msg, toUser, response.Header.CallId);
            }
        }

        /// <summary>
        /// 获取SDP协议中SessionName字段值
        /// </summary>
        /// <param name="body">SDP文本</param>
        /// <returns></returns>
        private string GetSessionName(string body)
        {
            string[] text = body.Split('\n');
            foreach (string item in text)
            {
                string[] values = item.Split('=');
                if (values.Contains("s"))
                {
                    if (values[1] == "Play\r" || values[1] == "Playback\r")
                    {
                        return values[1];
                    }
                }
                else if (values.Contains("t"))
                {
                    if (values[1] == "0 0\r")
                    {
                        return CommandType.Play.ToString();
                    }
                    else
                    {
                        return CommandType.Playback.ToString();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 失败的请求
        /// </summary>
        /// <param name="msg">错误消息</param>
        /// <param name="user">设备编码</param>
        /// <param name="callId">呼叫编号</param>
        private void BadRequest(string msg, string user, string callId)
        {
            for (int i = 0; i < 2; i++)
            {
                CommandType cmdType = CommandType.Unknown;
                if (i == 0)
                {
                    cmdType = CommandType.Play;
                }
                else
                {
                    cmdType = CommandType.Playback;
                }
                string key = user + cmdType;
                MonitorService[key].BadRequest(msg, callId);
            }
        }

        private void monitor_OnSIPServiceChanged(string msg, SipServiceStatus state)
        {
            OnSIPServiceChange(msg, state);
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
                response.Header.UserAgent = UserAgent;
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
        /// 设备目录查询
        /// </summary>
        /// <param name="deviceId">目的设备编码</param>
        public void DeviceCatalogQuery()
        {
            if (LocalEndPoint == null)
            {
                OnSIPServiceChange(RemoteSIPId, SipServiceStatus.Wait);
                return;
            }
            lock (RemoteTrans)
            {
                foreach (var trans in RemoteTrans)
                {
                    SIPEndPoint remoteEndPoint = SIPEndPoint.ParseSIPEndPoint(trans.Key);

                    SIPRequest catalogReq = QueryItems(remoteEndPoint, trans.Value);
                    CatalogQuery catalog = new CatalogQuery()
                    {
                        CommandType = CommandType.Catalog,
                        DeviceID = trans.Value,
                        SN = new Random().Next(1, ushort.MaxValue)
                    };
                    string xmlBody = CatalogQuery.Instance.Save<CatalogQuery>(catalog);
                    catalogReq.Body = xmlBody;
                    Transport.SendRequest(remoteEndPoint, catalogReq);
                }
            }
            //_catalogTask = new TaskTiming(catalogReq, Transport);
            //this.SendRequestTimeout += _catalogTask.MessageSendRequestTimeout;
            //_catalogTask.Start();
        }

        /// <summary>
        /// 查询设备目录请求
        /// </summary>
        /// <returns></returns>
        private SIPRequest QueryItems(SIPEndPoint remoteEndPoint, string remoteSIPId)
        {
            string fromTag = CallProperties.CreateNewTag();
            string toTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();

            SIPURI remoteUri = new SIPURI(remoteSIPId, remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(LocalSIPId, LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, toTag);
            SIPRequest catalogReq = Transport.GetRequest(SIPMethodsEnum.MESSAGE, remoteUri);
            catalogReq.Header.From = from;
            catalogReq.Header.Contact = null;
            catalogReq.Header.Allow = null;
            catalogReq.Header.To = to;
            catalogReq.Header.UserAgent = UserAgent;
            catalogReq.Header.CSeq = cSeq;
            catalogReq.Header.CallId = callId;
            catalogReq.Header.ContentType = "application/MANSCDP+xml";

            return catalogReq;
        }

        public void Stop()
        {
            if (_catalogTask != null)
            {
                _catalogTask.Stop();
            }
            foreach (var item in MonitorService)
            {
                item.Value.Stop();
            }
            LocalEndPoint = null;
            LocalSIPId = null;
            RemoteEndPoint = null;
            RemoteSIPId = null;
            Transport = null;
            MonitorService.Clear();
            MonitorService = null;
        }

        /// <summary>
        /// 设置媒体(rtp/rtcp)端口号
        /// </summary>
        /// <returns></returns>
        public int[] SetMediaPort()
        {
            var inUseUDPPorts = (from p in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port >= MEDIA_PORT_START select p.Port).OrderBy(x => x).ToList();

            int rtpPort = 0;
            int rtcpPort = 0;

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
                        rtcpPort = index;
                        break;
                    }
                }
            }
            else
            {
                rtpPort = MEDIA_PORT_START;
                rtcpPort = MEDIA_PORT_START + 1;
            }

            if (MEDIA_PORT_START >= MEDIA_PORT_END)
            {
                MEDIA_PORT_START = 10000;
            }
            MEDIA_PORT_START += 2;
            int[] mediaPort = new int[2];
            mediaPort[0] = rtpPort;
            mediaPort[1] = rtcpPort;
            return mediaPort;
        }
    }
}
