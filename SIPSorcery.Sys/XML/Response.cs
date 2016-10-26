using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 处理消息响应
    /// </summary>
    [XmlRoot("Response")]
    public class Response : XmlHelper<Response>
    {
        private static Response instance;

        /// <summary>
        /// 以单例模式访问
        /// </summary>
        public static Response Instance
        {
            get
            {
                if (instance == null)
                    instance = new Response();
                return instance;
            }
        }

        /// <summary>
        /// 指令
        /// </summary>
        [XmlElement("Variable")]
        public VariableType Variable { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        [XmlElement("Result")]
        public int Result { get; set; }
    }

    /// <summary>
    /// 心跳通知
    /// </summary>
    [XmlRoot("Action")]
    public class KeepAliveReq:XmlHelper<KeepAliveReq>
    {
        private static KeepAliveReq _instance;

        public static KeepAliveReq Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KeepAliveReq();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 心跳通知
        /// </summary>
        [XmlElement("Notify")]
        public Notify NotifyMsg { get; set; }
        
        public class Notify
        {
            /// <summary>
            /// 指令(KeepAlive)
            /// </summary>
            [XmlElement("Variable")]
            public VariableType Variable { get; set; }
        }
    }
}
