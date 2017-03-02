using log4net;
using SIPSorcery.GB28181.Net;
using SIPSorcery.GB28181.Servers.Cores.Packet;
using SIPSorcery.GB28181.Servers.SIPMessage;
using SIPSorcery.GB28181.SIP;
using SIPSorcery.GB28181.Sys;
using SIPSorcery.GB28181.Sys.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SIPSorcery.GB28181.Servers.SIPMonitor
{
    #region 云台控制命令
    /// <summary>
    /// 云台控制命令
    /// </summary>
    public enum PTZCommand : int
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 上
        /// </summary>
        [Description("上")]
        Up = 1,
        /// <summary>
        /// 左上
        /// </summary>
        [Description("左上")]
        UpLeft = 2,
        /// <summary>
        /// 右下
        /// </summary>
        [Description("右上")]
        UpRight = 3,
        /// <summary>
        /// 下
        /// </summary>
        [Description("下")]
        Down = 4,
        /// <summary>
        /// 左下
        /// </summary>
        [Description("左下")]
        DownLeft = 5,
        /// <summary>
        /// 右下
        /// </summary>
        [Description("右下")]
        DownRight = 6,
        /// <summary>
        /// 左
        /// </summary>
        [Description("左")]
        Left = 7,
        /// <summary>
        /// 右
        /// </summary>
        [Description("右")]
        Right = 8,
        /// <summary>
        /// 聚焦+
        /// </summary>
        [Description("聚焦+")]
        Focus1 = 9,
        /// <summary>
        /// 聚焦-
        /// </summary>
        [Description("聚焦-")]
        Focus2 = 10,
        /// <summary>
        /// 变倍+
        /// </summary>
        [Description("变倍+")]
        Zoom1 = 11,
        /// <summary>
        /// 变倍-
        /// </summary>
        [Description("变倍-")]
        Zoom2 = 12,
        /// <summary>
        /// 光圈开
        /// </summary>
        [Description("光圈Open")]
        Iris1 = 13,
        /// <summary>
        /// 光圈关
        /// </summary>
        [Description("光圈Close")]
        Iris2 = 14
    } 
    #endregion

    /// <summary>
    /// sip监控核心处理
    /// </summary>
    public class SIPMonitorCore : ISIPMonitorService
    {
        private static ILog logger = AppState.logger;
        private SIPMessageCore _msgCore;
        /// <summary>
        /// 远程终结点
        /// </summary>
        private SIPEndPoint _remoteEndPoint;
        /// <summary>
        /// rtp数据通道
        /// </summary>
        private RTPChannel _rtpChannel;
        private string _deviceId;
        private string _deviceName;
        private TaskTiming _realTask;
        private TaskTiming _byeTask;
        private SIPRequest _realReqSession;
        private int[] _mediaPort;
        private string _okTag;
        private SIPContactHeader _contact;
        private SIPViaSet _via;
        private int _recordTotal = -1;

        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<string, SipServiceStatus> OnSIPServiceChanged;

        /// <summary>
        /// 视频流回调
        /// </summary>
        public event Action<RTPFrame> OnStreamReady;

        /// <summary>
        /// 失败的请求
        /// </summary>
        public event Action OnBadRequest;

        public SIPMonitorCore(SIPMessageCore msgCore, string deviceId, string name, SIPEndPoint remoteEndPoint)
        {
            _msgCore = msgCore;
            _deviceId = deviceId;
            _deviceName = name;
            _remoteEndPoint = remoteEndPoint;
        }

        private FileStream m_fs = null;

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

        #region 实时视频
        /// <summary>
        /// 实时视频请求
        /// </summary>
        /// <param name="deviceId">设备编码</param>
        public void RealVideoReq()
        {
            lock (_msgCore.RemoteTrans)
            {
                if (!_msgCore.RemoteTrans.ContainsKey(_remoteEndPoint.ToString()))
                {
                    OnSIPServiceChange(_deviceName + "-" + _deviceId + _remoteEndPoint.ToString(), SipServiceStatus.Wait);
                    return;
                }
            }

            _mediaPort = _msgCore.SetMediaPort();
            this.Stop();
            ByeVideoReq();
            SIPRequest realReq = RealVideoReq(_mediaPort);
            _msgCore.Transport.SendRequest(_remoteEndPoint, realReq);

            //_realTask = new TaskTiming(realReq, _msgCore.Transport);
            //_msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            //_realTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            //_realTask.Start();
        }

        private void Task_OnCloseRTPChannel()
        {
            this.Stop();
        }

        /// <summary>
        /// 确认接收视频请求
        /// </summary>
        /// <param name="response">响应消息</param>
        /// <returns></returns>
        public void AckRequest(SIPResponse response)
        {
            _rtpChannel = new RTPChannel(_remoteEndPoint.GetIPEndPoint(), _mediaPort[0], _mediaPort[1], FrameTypesEnum.H264);
            _rtpChannel.OnFrameReady += _rtpChannel_OnFrameReady;
            _rtpChannel.Start();

            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
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
            _okTag = response.Header.To.ToTag;
            _contact = header.Contact.FirstOrDefault();
            _via = header.Vias;
            _msgCore.Transport.SendRequest(_remoteEndPoint, ackReq);
        }

        private void _rtpChannel_OnFrameReady(RTPFrame frame)
        {
            if (OnStreamReady != null)
            {
                OnStreamReady(frame);
            }
            byte[] buffer = frame.GetFramePayload();
            PsToH264(buffer);
            //foreach (var item in frame.FramePackets)
            //{
            //    logger.Debug("Seq:" + item.Header.SequenceNumber + "----Timestamp:" + item.Header.Timestamp);
            //}
            //if (this.m_fs == null)
            //{
            //    this.m_fs = new FileStream("D:\\" + _deviceId + ".h264", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 50 * 1024);
            //}
            //m_fs.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 失败的请求
        /// </summary>
        /// <param name="msg">失败消息内容</param>
        /// <param name="callId">呼叫编号</param>
        public void BadRequest(string msg, string callId)
        {
            if (_realReqSession == null)
            {
                return;
            }
            if (_realReqSession.Header.CallId == callId)
            {

                this.Stop();
                if (OnBadRequest != null)
                {
                    OnBadRequest();
                }
                _realReqSession = null;
            }
        }

        /// <summary>
        /// 实时视频请求
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">端口号</param>
        /// <param name="fromTag">from tag</param>
        /// <param name="cSeq">序号</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        private SIPRequest RealVideoReq(int[] mediaPort)
        {
            string localIp = _msgCore.LocalEndPoint.Address.ToString();
            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();

            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
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


        /// <summary>
        /// 结束实时视频请求
        /// </summary>
        public void ByeVideoReq()
        {
            if (_realReqSession == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId + _remoteEndPoint.ToString(), SipServiceStatus.Wait);
                return;
            }
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, _realReqSession.Header.From.FromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, _okTag);
            SIPRequest byeReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.BYE, remoteUri);
            SIPHeader header = new SIPHeader(from, to, _realReqSession.Header.CSeq, _realReqSession.Header.CallId);
            header.CSeqMethod = byeReq.Header.CSeqMethod;
            header.Vias = _via;
            header.MaxForwards = byeReq.Header.MaxForwards;
            header.UserAgent = _msgCore.UserAgent;
            header.Contact = _realReqSession.Header.Contact;
            header.Contact.Clear();
            header.CSeq = _realReqSession.Header.CSeq + 1;
            header.Contact.Add(_contact);
            byeReq.Header.From = from;
            byeReq.Header = header;
            this.Stop();
            _msgCore.Transport.SendRequest(_remoteEndPoint, byeReq);

            //_byeTask = new TaskTiming(byeReq, _msgCore.Transport);
            //_msgCore.SendRequestTimeout += _byeTask.MessageSendRequestTimeout;
            //_byeTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            //_byeTask.Start();

        }

        /// <summary>
        /// 停止计时器/关闭RTP通道
        /// </summary>
        public void Stop()
        {
            if (_realTask != null)
            {
                _realTask.OnCloseRTPChannel -= Task_OnCloseRTPChannel;
                _realTask.Stop();
            }
            if (_byeTask != null)
            {
                _byeTask.OnCloseRTPChannel -= Task_OnCloseRTPChannel;
                _byeTask.Stop();
            }
            if (_rtpChannel != null)
            {
                _rtpChannel.OnFrameReady -= _rtpChannel_OnFrameReady;
                _rtpChannel.Close();
            }
            if (m_fs != null)
            {
                m_fs.Close();
                m_fs.Dispose();
                m_fs = null;
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
            sdp.SessionName = CommandType.Play.ToString();
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
        /// 设置sip主题
        /// </summary>
        /// <returns></returns>
        private string SetSubject()
        {
            return _deviceId + ":" + 0 + "," + _msgCore.LocalSIPId + ":" + new Random().Next(100, ushort.MaxValue);
        } 
        #endregion

        #region 处理PS数据

        private byte[] _publicByte = new byte[0];
        public void PsToH264(byte[] buffer)
        {
            _publicByte = copybyte(_publicByte, buffer);
            int i = 0;
            int BANum = 0;
            int startIndex = 0;
            if (buffer == null || buffer.Length < 5)
            {
                return;
            }
            int bytes = _publicByte.Length - 4;
            while (i < bytes)
            {
                if (_publicByte[i] == 0x00 && _publicByte[i + 1] == 0x00 && _publicByte[i + 2] == 0x01 && _publicByte[i + 3] == 0xBA)
                {
                    BANum++;
                    if (BANum == 1)
                    {
                        startIndex = i;
                    }
                    if (BANum == 2)
                    {
                        break;
                    }
                }
                i++;
            }

            if (BANum == 2)
            {
                 int esNum = i - startIndex;
                byte[] psByte = new byte[esNum];
                Array.Copy(_publicByte, startIndex, psByte, 0, esNum);

                try
                {
                   
                    //处理psByte
                    doPsByte(psByte);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("===============" + ex.Message + ex.StackTrace.ToString());
                }

                byte[] overByte = new byte[_publicByte.Length - i];
                Array.Copy(_publicByte, i, overByte, 0, overByte.Length);
                _publicByte = overByte;
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
            if (!(psDate[0] == 0 && psDate[1] == 0 && psDate[2] == 1 && psDate[3] == 0xBA))
            {
                Console.WriteLine("出错了！！！！！！！！");
            }
            long scr = 0;
            Stream msStream = new System.IO.MemoryStream(psDate);

            var ph = new PSPacketHeader(msStream);
            scr = ph.GetSCR();
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
            try
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
                long tick = videoPESList.FirstOrDefault().GetVideoTimetick();
                var esdata = stream.ToArray();
                stream.Close();
                videoPESList.Clear();

                //if (this.m_fs == null)
                //{
                //    this.m_fs = new FileStream("D:\\" + _deviceId + ".h264", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 50 * 1024);
                //}
                //m_fs.Write(esdata, 0, esdata.Length);

            }
            catch (Exception ex)
            {
                //ComHelper.Log.Write(ex, "PsanalyzeOther");
            }
        }


        #endregion

        #region <<<<<<demon  录像功能>>>>>

        public void RecordQueryTotal(int recordTotal)
        {
            _recordTotal = recordTotal;
        }

        /// <summary>
        /// 录像文件查询
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public int RecordFileQuery(DateTime startTime, DateTime endTime)
        {
            lock (_msgCore.RemoteTrans)
            {
                if (!_msgCore.RemoteTrans.ContainsKey(_remoteEndPoint.ToString()))
                {
                    OnSIPServiceChange(_deviceName + "-" + _deviceId + _remoteEndPoint.ToString(), SipServiceStatus.Wait);
                    return 0;
                }
            }


            this.Stop();

            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, CallProperties.CreateNewTag());
            SIPToHeader to = new SIPToHeader(null, remoteUri, CallProperties.CreateNewTag());
            SIPRequest recordFileReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.MESSAGE, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            recordFileReq.Header.Contact.Clear();
            recordFileReq.Header.Contact.Add(contactHeader);

            recordFileReq.Header.Allow = null;
            recordFileReq.Header.From = from;
            recordFileReq.Header.To = to;
            recordFileReq.Header.UserAgent = _msgCore.UserAgent;
            recordFileReq.Header.CSeq = CallProperties.CreateNewCSeq();
            recordFileReq.Header.CallId = CallProperties.CreateNewCallId();
            recordFileReq.Header.ContentType = "application/MANSCDP+xml";

            string bTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss");
            string eTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss");
            RecordQuery record = new RecordQuery()
            {
                DeviceID = _deviceId,
                SN = new Random().Next(1, 3000),
                CmdType = CommandType.RecordInfo,
                Secrecy = 0,
                StartTime = bTime,
                EndTime = eTime,
                Type = "time"
            };

            _recordTotal = -1;
            string xmlBody = RecordQuery.Instance.Save<RecordQuery>(record);
            recordFileReq.Body = xmlBody;
            _msgCore.Transport.SendRequest(_remoteEndPoint, recordFileReq);
            DateTime recordQueryTime = DateTime.Now;
            while (_recordTotal < 0)
            {
                Thread.Sleep(50);
                if (DateTime.Now.Subtract(recordQueryTime).TotalSeconds > 2)
                {
                    logger.Debug(_deviceName + "[" + _deviceId + "] 等待录像查询超时");
                    _recordTotal = 0;
                    break;
                }
            }

            return _recordTotal;
        }

        /// <summary>
        /// 录像点播视频请求
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public void BackVideoReq(DateTime beginTime, DateTime endTime)
        {
            lock (_msgCore.RemoteTrans)
            {
                if (!_msgCore.RemoteTrans.ContainsKey(_remoteEndPoint.ToString()))
                {
                    OnSIPServiceChange(_deviceName + "-" + _deviceId + _remoteEndPoint.ToString(), SipServiceStatus.Wait);
                    return;
                }
            }
            //if (_mediaPort == null)
            //{
            //    _mediaPort = _msgCore.SetMediaPort();

            //}
            _mediaPort = _msgCore.SetMediaPort();
            uint startTime = TimeConvert.DateToTimeStamp(beginTime);
            uint stopTime = TimeConvert.DateToTimeStamp(endTime);

            this.Stop();
            SIPRequest realReq = BackVideoReq(_mediaPort, startTime, stopTime);
            _msgCore.Transport.SendRequest(_remoteEndPoint, realReq);
            //_realTask = new TaskTiming(realReq, _msgCore.Transport);
            //_msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            //_realTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            //_realTask.Start();
        }

        /// <summary>
        /// 录像点播请求
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">端口号</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="stopTime">结束时间</param>
        /// <param name="fromTag">from tag</param>
        /// <param name="cSeq">序号</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        private SIPRequest BackVideoReq(int[] mediaPort, uint startTime, uint stopTime)
        {
            string localIp = _msgCore.LocalEndPoint.Address.ToString();
            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();

            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest backReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INVITE, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            backReq.Header.Contact.Clear();
            backReq.Header.Contact.Add(contactHeader);

            backReq.Header.Allow = null;
            backReq.Header.From = from;
            backReq.Header.To = to;
            backReq.Header.UserAgent = _msgCore.UserAgent;
            backReq.Header.CSeq = cSeq;
            backReq.Header.CallId = callId;
            backReq.Header.Subject = SetSubject();
            backReq.Header.ContentType = "application/sdp";

            backReq.Body = SetMediaReq(localIp, mediaPort, startTime, stopTime);
            _realReqSession = backReq;
            return backReq;
        }

        /// <summary>
        /// 设置媒体参数请求(回放)
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">rtp/rtcp媒体端口(10000/10001)</param>
        /// <param name="startTime">录像开始时间</param>
        /// <param name="stopTime">录像结束数据</param>
        /// <returns></returns>
        private string SetMediaReq(string localIp, int[] mediaPort, uint startTime, uint stopTime)
        {
            SDPConnectionInformation sdpConn = new SDPConnectionInformation(localIp);

            SDP sdp = new SDP();
            sdp.Version = 0;
            sdp.SessionId = "0";
            sdp.Username = _msgCore.LocalSIPId;
            sdp.SessionName = CommandType.Playback.ToString();
            sdp.Connection = sdpConn;
            sdp.Timing = startTime + " " + stopTime;
            sdp.Address = localIp;
            sdp.URI = _deviceId + ":" + 1;

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
        /// 结束录像点播视频请求
        /// </summary>
        /// <returns></returns>
        public void BackVideoStopPlayingControlReq()
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
            this.Stop();
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, byeReq);
            _byeTask = new TaskTiming(byeReq, _msgCore.Transport);
            _msgCore.SendRequestTimeout += _byeTask.MessageSendRequestTimeout;
            _byeTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            _byeTask.Start();
        }
        /// <summary>
        /// 控制录像点播播放速度
        /// </summary>
        /// <param name="scale">播放速度</param>
        /// <param name="range">时间范围</param>
        public void BackVideoPlaySpeedControlReq(string scale, DateTime range)
        {
            if (_msgCore.LocalEndPoint == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }
            if (_mediaPort == null)
            {
                _mediaPort = _msgCore.SetMediaPort();

            }

            uint time = TimeConvert.DateToTimeStamp(range);
            string localIp = _msgCore.LocalEndPoint.Address.ToString();
            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();

            //this.Stop();
            SIPRequest realReq = BackVideoPlaySpeedControlReq(localIp, _mediaPort, scale, time, fromTag, cSeq, callId);
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, realReq);
            _realTask = new TaskTiming(realReq, _msgCore.Transport);
            _msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            _realTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            _realTask.Start();
        }
        /// <summary>
        /// 控制录像点播播放速度
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">端口号</param>
        /// <param name="scale">播放速率</param>
        /// <param name="Time">时间范围</param>
        /// <param name="fromTag">from tag</param>
        /// <param name="cSeq">序号</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        private SIPRequest BackVideoPlaySpeedControlReq(string localIp, int[] mediaPort, string scale, uint Time, string fromTag, int cSeq, string callId)
        {
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest backReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INFO, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            backReq.Header.Contact.Clear();
            backReq.Header.Contact.Add(contactHeader);

            backReq.Header.Allow = null;
            backReq.Header.From = from;
            backReq.Header.To = to;
            backReq.Header.UserAgent = _msgCore.UserAgent;
            backReq.Header.CSeq = cSeq;
            backReq.Header.CallId = callId;
            backReq.Header.Subject = SetSubject();
            backReq.Header.ContentType = "application/mansrtsp";

            backReq.Body = SetMediaReq(localIp, mediaPort, scale, Time);
            _realReqSession = backReq;
            return backReq;
        }
        /// <summary>
        /// 设置媒体参数请求(回放 播放速率)
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">rtp/rtcp媒体端口(10000/10001)</param>
        /// <param name="scale">播放速率</param>
        /// <param name="Time">时间范围</param>
        /// <returns></returns>
        private string SetMediaReq(string localIp, int[] mediaPort, string scale, uint Time)
        {
            string str = string.Empty;
            str += "PALY MANSRTSP/1.0" + "\r\n";
            str += "CSeq: 2" + "\r\n";
            str += "Scal: 2" + "\r\n";
            str += "Range: npt=now-" + "\r\n";
            return str;
        }
        /// <summary>
        /// 恢复录像播放
        /// </summary>
        public void BackVideoContinuePlayingControlReq()
        {
            if (_msgCore.LocalEndPoint == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }
            if (_mediaPort == null)
            {
                _mediaPort = _msgCore.SetMediaPort();

            }

            string localIp = _msgCore.LocalEndPoint.Address.ToString();
            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();
            //this.Stop();
            SIPRequest realReq = BackVideoContinuePlayingControlReq(localIp, _mediaPort, fromTag, cSeq, callId);
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, realReq);
            _realTask = new TaskTiming(realReq, _msgCore.Transport);
            _msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            _realTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            _realTask.Start();
        }
        /// <summary>
        /// 恢复录像播放
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">端口号</param>
        /// <param name="fromTag">from tag</param>
        /// <param name="cSeq">序号</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        public SIPRequest BackVideoContinuePlayingControlReq(string localIp, int[] mediaPort, string fromTag, int cSeq, string callId)
        {
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest backReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INFO, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            backReq.Header.Contact.Clear();
            backReq.Header.Contact.Add(contactHeader);

            backReq.Header.Allow = null;
            backReq.Header.From = from;
            backReq.Header.To = to;
            backReq.Header.UserAgent = _msgCore.UserAgent;
            backReq.Header.CSeq = cSeq;
            backReq.Header.CallId = callId;
            backReq.Header.Subject = SetSubject();
            backReq.Header.ContentType = "application/mansrtsp";

            backReq.Body = SetMediaPauseReq();
            _realReqSession = backReq;
            return backReq;
        }
        /// <summary>
        /// 设置录像恢复播放包体信息
        /// </summary>
        /// <returns></returns>
        private string SetMediaResumeReq()
        {
            string str = string.Empty;
            str += "PALY MANSRTSP/1.0" + "\r\n";
            str += "CSeq: 2" + "\r\n";
            str += "Scal: 1" + "\r\n";
            str += "Range: npt=now-" + "\r\n";
            return str;
        }
        /// <summary>
        /// 暂停录像播放
        /// </summary>
        public void BackVideoPauseControlReq()
        {
            if (_msgCore.LocalEndPoint == null)
            {
                OnSIPServiceChange(_deviceName + "-" + _deviceId, SipServiceStatus.Wait);
                return;
            }
            if (_mediaPort == null)
            {
                _mediaPort = _msgCore.SetMediaPort();

            }

            string localIp = _msgCore.LocalEndPoint.Address.ToString();
            string fromTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();
            //this.Stop();
            SIPRequest realReq = BackVideoPauseControlReq(localIp, _mediaPort, fromTag, cSeq, callId);
            _msgCore.Transport.SendRequest(_msgCore.RemoteEndPoint, realReq);
            _realTask = new TaskTiming(realReq, _msgCore.Transport);
            _msgCore.SendRequestTimeout += _realTask.MessageSendRequestTimeout;
            _realTask.OnCloseRTPChannel += Task_OnCloseRTPChannel;
            _realTask.Start();
        }
        /// <summary>
        /// 暂停录像播放
        /// </summary>
        /// <param name="localIp">本地ip</param>
        /// <param name="mediaPort">端口号</param>
        /// <param name="fromTag">from tag</param>
        /// <param name="cSeq">序号</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        public SIPRequest BackVideoPauseControlReq(string localIp, int[] mediaPort, string fromTag, int cSeq, string callId)
        {
            SIPURI remoteUri = new SIPURI(_deviceId, _msgCore.RemoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, null);
            SIPRequest backReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.INFO, remoteUri);
            SIPContactHeader contactHeader = new SIPContactHeader(null, localUri);
            backReq.Header.Contact.Clear();
            backReq.Header.Contact.Add(contactHeader);

            backReq.Header.Allow = null;
            backReq.Header.From = from;
            backReq.Header.To = to;
            backReq.Header.UserAgent = _msgCore.UserAgent;
            backReq.Header.CSeq = cSeq;
            backReq.Header.CallId = callId;
            backReq.Header.Subject = SetSubject();
            backReq.Header.ContentType = "application/mansrtsp";

            backReq.Body = SetMediaPauseReq();
            _realReqSession = backReq;
            return backReq;
        }
        /// <summary>
        /// 设置录像暂停包体信息
        /// </summary>
        /// <returns></returns>
        private string SetMediaPauseReq()
        {
            string str = string.Empty;
            str += "PAUSE MANSRTSP/1.0" + "\r\n";
            str += "CSeq: 8" + "\r\n";
            //str += "Scal: 2" + "\r\n";
            str += "PauseTime: 15" + "\r\n";
            return str;
        }

        #endregion

        #region PTZ云台控制

        /// <summary>
        /// PTZ云台控制
        /// </summary>
        /// <param name="ucommand">控制命令</param>
        /// <param name="dwStop">开始或结束</param>
        /// <param name="dwSpeed">速度</param>
        public void PtzContrl(int ucommand, int dwStop, int dwSpeed)
        {
            lock (_msgCore.RemoteTrans)
            {
                if (!_msgCore.RemoteTrans.ContainsKey(_remoteEndPoint.ToString()))
                {
                    OnSIPServiceChange(_deviceName + "-" + _deviceId + _remoteEndPoint.ToString(), SipServiceStatus.Wait);
                    return;
                }
            }

            string fromTag = CallProperties.CreateNewTag();
            string toTag = CallProperties.CreateNewTag();
            int cSeq = CallProperties.CreateNewCSeq();
            string callId = CallProperties.CreateNewCallId();
            SIPRequest catalogReq = PTZRequest(fromTag, toTag, cSeq, callId);
            string cmdStr = GetPtzCmd(ucommand, dwStop, dwSpeed);

            PTZControl ptz = new PTZControl()
            {
                CommandType = CommandType.DeviceControl,
                DeviceID = _deviceId,
                SN = new Random().Next(9999),
                PTZCmd = cmdStr
            };
            string xmlBody = PTZControl.Instance.Save<PTZControl>(ptz);
            catalogReq.Body = xmlBody;
            _msgCore.Transport.SendRequest(_remoteEndPoint, catalogReq);

        }

        /// <summary>
        /// 查询设备目录请求
        /// </summary>
        /// <returns></returns>
        private SIPRequest PTZRequest(string fromTag, string toTag, int cSeq, string callId)
        {
            SIPURI remoteUri = new SIPURI(_deviceId, _remoteEndPoint.ToHost(), "");
            SIPURI localUri = new SIPURI(_msgCore.LocalSIPId, _msgCore.LocalEndPoint.ToHost(), "");
            SIPFromHeader from = new SIPFromHeader(null, localUri, fromTag);
            SIPToHeader to = new SIPToHeader(null, remoteUri, toTag);
            SIPRequest catalogReq = _msgCore.Transport.GetRequest(SIPMethodsEnum.MESSAGE, remoteUri);
            catalogReq.Header.From = from;
            catalogReq.Header.Contact = null;
            catalogReq.Header.Allow = null;
            catalogReq.Header.To = to;
            catalogReq.Header.UserAgent = _msgCore.UserAgent;
            catalogReq.Header.CSeq = cSeq;
            catalogReq.Header.CallId = callId;
            catalogReq.Header.ContentType = "application/MANSCDP+xml";
            return catalogReq;
        }


        /// <summary>
        /// 拼接ptz控制指令
        /// </summary>
        /// <param name="ucommand"></param>
        /// <param name="dwStop"></param>
        /// <param name="dwSpeed"></param>
        /// <returns></returns>
        private string GetPtzCmd(int ucommand, int dwStop, int dwSpeed)
        {
            List<int> cmdList = new List<int>(8);
            cmdList.Add(0xA5);
            cmdList.Add(0x0F);
            cmdList.Add(0x01);
            if (dwStop == 1)//停止云台控制
            {
                cmdList.Add(00);
                cmdList.Add(00);
                cmdList.Add(00);
                cmdList.Add(00);
                cmdList.Add(0xB5);
            }
            else//开始云台控制
            {
                switch ((PTZCommand)ucommand)
                {
                    case PTZCommand.Up:
                        cmdList.Add(0x08);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Down:
                        cmdList.Add(0x04);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Left:
                        cmdList.Add(0x02);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Right:
                        cmdList.Add(0x01);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.UpRight:
                        cmdList.Add(0x9);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.DownRight:
                        cmdList.Add(0x09);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.UpLeft:
                        cmdList.Add(0x0A);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.DownLeft:
                        cmdList.Add(0x06);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Zoom1://镜头放大
                        cmdList.Add(0x10);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed << 4);
                        break;
                    case PTZCommand.Zoom2://镜头缩小
                        cmdList.Add(0x20);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed << 4);
                        break;
                    case PTZCommand.Focus1://聚焦+
                        cmdList.Add(0x42);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Focus2://聚焦—
                        cmdList.Add(0x41);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Iris1: //光圈open
                        cmdList.Add(0x44);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    case PTZCommand.Iris2: //光圈close
                        cmdList.Add(0x48);
                        cmdList.Add(00);
                        cmdList.Add(dwSpeed);
                        cmdList.Add(00);
                        break;
                    default:
                        break;
                }
            }

            int checkBit = 0;
            foreach (int cmdItem in cmdList)
            {
                checkBit = checkBit + cmdItem;
            }
            checkBit = checkBit % 256;
            cmdList.Add(checkBit);

            string cmdStr = string.Empty;
            foreach (var cmdItemStr in cmdList)
            {
                cmdStr = cmdStr + cmdItemStr.ToString("X").PadLeft(2, '0');
            }
            return cmdStr;
        }
        #endregion
    }
}
