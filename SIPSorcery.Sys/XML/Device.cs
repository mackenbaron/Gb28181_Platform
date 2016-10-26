using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 前端设备查询
    /// </summary>
    [XmlRoot("Action")]
    public class Device : XmlHelper<Device>
    {
        private static Device _instance;
        public static Device Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Device();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 操作指令(DeviceInfo)
        /// </summary>
        [XmlElement("Variable")]
        public VariableType Variable { get; set; }
        /// <summary>
        /// 权限功能码
        /// </summary>
        [XmlElement("Privilege")]
        public int Privilege { get; set; }
    }

    /// <summary>
    /// 查询设备信息响应
    /// </summary>
    [XmlRoot("Response")]
    public class DeviceRes : XmlHelper<DeviceRes>
    {
        private static DeviceRes _instance;

        public static DeviceRes Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceRes();
                }
                return _instance;
            }
        }

        [XmlElement("QueryResponse")]
        public QueryResponse Query { get; set; }

        /// <summary>
        /// 查询结果
        /// </summary>
        public class QueryResponse
        {
            /// <summary>
            /// 指令(DeviceInfo)
            /// </summary>
            [XmlElement("Variable")]
            public VariableType Variable { get; set; }
            /// <summary>
            /// 结果
            /// </summary>
            [XmlElement("Result")]
            public int Result { get; set; }
            /// <summary>
            /// 生产厂家
            /// </summary>
            [XmlElement("Manufacturer")]
            public string Manufacturer { get; set; }
            /// <summary>
            /// 型号
            /// </summary>
            [XmlElement("Model")]
            public string Model { get; set; }
            /// <summary>
            /// 固件版本
            /// </summary>
            [XmlElement("Firmware")]
            public string Firmware { get; set; }
            /// <summary>
            /// 最多相机数量
            /// </summary>
            [XmlElement("MaxCamera")]
            public int MaxCamera { get; set; }
        }
    }
}
