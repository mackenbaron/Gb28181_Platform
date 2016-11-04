using SIPSorcery.GB28181.Servers.SIPMessage;
using SIPSorcery.GB28181.SIP;
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
        SIPRequest AckRequest(SIPResponse response);

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
        /// <param name="response">失败消息</param>
        /// <returns></returns>
        void BadRequest(string msg);

        /// <summary>
        /// 视频流回调完成
        /// </summary>
        event Action<byte[]> OnStreamReady;
    }
}
