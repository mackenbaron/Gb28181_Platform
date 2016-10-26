using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 实时视频请求
    /// </summary>
    [XmlRoot("Action")]
    public class RealVideo : XmlHelper<RealVideo>
    {
        private static RealVideo _instance;

        /// <summary>
        /// 以单例模式访问
        /// </summary>
        public static RealVideo Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RealVideo();
                return _instance;
            }
        }

        /// <summary>
        /// 编码地址
        /// </summary>
        [XmlElement("Address")]
        public string Address { get; set; }

        /// <summary>
        /// 指令(RealMedia)
        /// </summary>
        [XmlElement("Variable")]
        public VariableType Variable { get; set; }
        /// <summary>
        /// 权限功能码
        /// </summary>
        [XmlElement("Privilege")]
        public int Privilege { get; set; }
        /// <summary>
        /// 源联网单元支持的码流格式(4CIF CIF QCIF)
        /// </summary>
        [XmlElement("Format")]
        public string Format { get; set; }
        /// <summary>
        /// 视频编码类型(H.264)
        /// </summary>
        [XmlElement("Video")]
        public string Video { get; set; }
        /// <summary>
        /// 音频编码类型(G.711)
        /// </summary>
        [XmlElement("Audio")]
        public string Audio { get; set; }
        /// <summary>
        /// 最高码率(800)
        /// </summary>
        [XmlElement("MaxBitrate")]
        public int MaxBitrate { get; set; }
        /// <summary>
        /// 接收视频的用户或视频转发代理的IP地址/传输协议/端口号
        /// 210.98.45.234 UDP 2350
        /// </summary>
        [XmlElement("Socket")]
        public string Socket { get; set; }
    }

    /// <summary>
    /// 实时视频响应
    /// </summary>
    [XmlRoot("Response")]
    public class RealVideoRes : XmlHelper<RealVideoRes>
    {
        private static RealVideoRes _instance;

        public static RealVideoRes Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RealVideoRes();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 指令(RealMedia)
        /// </summary>
        [XmlElement("Variable")]
        public VariableType Variable { get; set; }
        /// <summary>
        /// 源联网单元支持的码流格式(4CIF CIF QCIF)
        /// </summary>
        [XmlElement("Format")]
        public string Format { get; set; }
        /// <summary>
        /// 视频编码类型(H.264)
        /// </summary>
        [XmlElement("Video")]
        public string Video { get; set; }
        /// <summary>
        /// 音频编码类型(G.711)
        /// </summary>
        [XmlElement("Audio")]
        public string Audio { get; set; }
        /// <summary>
        /// 比特率
        /// </summary>
        [XmlElement("Bitrate")]
        public int Bitrate { get; set; }
        /// <summary>
        /// 接收视频的用户或视频转发代理的IP地址/传输协议/端口号
        /// 210.98.45.234 UDP 2350
        /// </summary>
        [XmlElement("Socket")]
        public string Socket { get; set; }
    }
}
