using SIPSorcery.GB28181.Servers.SIPMessage;
using SIPSorcery.GB28181.SIP;
using SIPSorcery.GB28181.Sys.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.GB28181.Servers
{
    /// <summary>
    /// 监控服务
    /// </summary>
    public interface ISIPMonitorService
    {
        /// <summary>
        /// 实时视频请求
        /// </summary>
        void RealVideoReq();

        /// <summary>
        /// 取消实时视频请求
        /// </summary>
        void ByeVideoReq();

        /// <summary>
        /// 确认接收实时视频请求
        /// </summary>
        /// <param name="response">sip响应</param>
        /// <returns>sip请求</returns>
        void AckRequest(SIPResponse response);

        /// <summary>
        /// sip服务状态
        /// </summary>
        event Action<string, SipServiceStatus> OnSIPServiceChanged;

        /// <summary>
        /// 停止监控服务
        /// </summary>
        void Stop();

        /// <summary>
        /// 失败的请求
        /// </summary>
        /// <param name="msg">失败消息</param>
        /// <param name="callId">呼叫编号</param>
        /// <returns></returns>
        void BadRequest(string msg,string callId);

        /// <summary>
        /// 视频流回调完成
        /// </summary>
        event Action<byte[]> OnStreamReady;

        /// <summary>
        /// 录像文件检索
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// </summary>
        void RecordFileQuery(DateTime beginTime,DateTime endTime);

        /// <summary>
        /// 录像点播视频请求
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        void BackVideoReq(DateTime beginTime, DateTime endTime);
        /// <summary>
        /// 录像点播视频播放速度控制请求
        /// </summary>
        /// <param name="scale">播放快进比例</param>
        /// <param name="range">视频播放时间段</param>
        void BackVideoPlaySpeedControlReq(string scale, DateTime range);
        /// <summary>
        /// 录像点播视频继续播放控制请求
        /// </summary>
        void BackVideoContinuePlayingControlReq();
        /// <summary>
        /// 录像点播视频暂停控制请求
        /// </summary>
        void BackVideoPauseControlReq();
        /// <summary>
        /// 录像点播视频停止播放控制请求
        /// </summary>
        void BackVideoStopPlayingControlReq();

        event Action OnBadRequest;
    }
}
