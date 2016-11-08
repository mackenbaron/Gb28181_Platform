using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.GB28181.Sys.XML
{
    /// <summary>
    /// 设备目录信息
    /// </summary>
    [XmlRoot("Response")]
    public class Catalog : XmlHelper<Catalog>
    {
        private static Catalog _instance;

        /// <summary>
        /// 单例模式访问
        /// </summary>
        public static Catalog Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Catalog();
                return _instance;
            }
        }

        /// <summary>
        /// 命令类型
        /// </summary>
        [XmlElement("CmdType")]
        public CommandType CmdType { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [XmlElement("SN")]
        public int SN { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        [XmlElement("DeviceID")]
        public string DeviceID { get; set; }

        /// <summary>
        /// 设备总条数
        /// </summary>
        [XmlElement("SumNum")]
        public int SumNum { get; set; }

        /// <summary>
        /// 列表显示条数
        /// </summary>
        [XmlElement("DeviceList")]
        public DevList DeviceList { get; set; }

        /// <summary>
        /// 设备列表
        /// </summary>
        public class DevList
        {
            private List<Item> _devItem = new List<Item>();

            /// <summary>
            /// 设备项
            /// </summary>
            [XmlElement("Item")]
            public List<Item> Items
            {
                get { return _devItem; }
            }
        }

        /// <summary>
        /// 设备信息
        /// </summary>
        public class Item
        {
            /// <summary>
            /// 设备编码
            /// </summary>
            [XmlElement("DeviceID")]
            public string DeviceID { get; set; }

            /// <summary>
            /// 设备名称
            /// </summary>
            [XmlElement("Name")]
            public string Name { get; set; }

            /// <summary>
            /// 目录类型
            /// </summary>
            [XmlElement("CatalogType")]
            public int CatalogType { get; set; }

            /// <summary>
            /// 制造商
            /// </summary>
            [XmlElement("Manufacturer")]
            public string Manufacturer { get; set; }

            /// <summary>
            /// 型号
            /// </summary>
            [XmlElement("Model")]
            public string Model { get; set; }

            /// <summary>
            /// 设备归属
            /// </summary>
            [XmlElement("Owner")]
            public string Owner { get; set; }

            /// <summary>
            /// 行政区域码
            /// </summary>
            [XmlElement("CivilCode")]
            public string CivilCode { get; set; }

            /// <summary>
            /// 警区
            /// </summary>
            [XmlElement("Block")]
            public string Block { get; set; }

            /// <summary>
            /// 安装地址
            /// </summary>
            [XmlElement("Address")]
            public string Address { get; set; }

            /// <summary>
            /// 是否有子设备，1有，0没有
            /// </summary>
            [XmlElement("Parental")]
            public byte Parental { get; set; }

            /// <summary>
            /// 父设备ID(可选)
            /// </summary>
            [XmlElement("ParentID")]
            public string ParentID { get; set; }

            /// <summary>
            /// 信令安全模式，0：不采用，2：S/MIME签名方式 3：S/MIME加密签名 4数字摘要方式(可选) 
            /// </summary>
            [XmlElement("SafetyWay")]
            public byte SafetyWay { get; set; }

            /// <summary>
            /// 符合sip3261标准的认证注册方式，2：基于口令的双向认证方式，3：基于数字证书的双向认证方式
            /// </summary>
            [XmlElement("RegisterWay")]
            public byte RegisterWay { get; set; }

            /// <summary>
            /// 证书序列号（可选）
            /// </summary>
            [XmlElement("CertNum")]
            public string CertNum { get; set; }

            /// <summary>
            /// 证书有效标志，0无效，1有效(可选)
            /// </summary>
            [XmlElement("Certifiable")]
            public byte Certifiable { get; set; }

            /// <summary>
            /// 证书无效原因码(可选)
            /// </summary>
            [XmlElement("ErrCode")]
            public int ErrCode { get; set; }

            /// <summary>
            /// 证书终止有效期(可选)
            /// </summary>
            [XmlElement("EndTime")]
            public string EndTime { get; set; }

            /// <summary>
            /// 保密属性，0：不涉密，1涉密
            /// </summary>
            [XmlElement("Secrecy")]
            public byte Secrecy { get; set; }

            /// <summary>
            /// 设备IP（可选）
            /// </summary>
            [XmlElement("IPAddress")]
            public string IPAddress { get; set; }

            /// <summary>
            /// 端口(可选)
            /// </summary>
            [XmlElement("Port")]
            public ushort Port { get; set; }

            /// <summary>
            /// 设备密码（可选）
            /// </summary>
            [XmlElement("Password")]
            public string Password { get; set; }

            /// <summary>
            /// 设备状态
            /// </summary>
            [XmlElement("Status")]
            public DevStatus Status { get; set; }

            /// <summary>
            /// 精度(可选)
            /// </summary>
            [XmlElement("Longitude")]
            public double Longitude { get; set; }

            /// <summary>
            /// 纬度(可选)
            /// </summary>
            [XmlElement("Latitude")]
            public double Latitude { get; set; }
        }

    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DevStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        ON = 0,
        /// <summary>
        /// 故障
        /// </summary>
        OFF = 1
    }

    /// <summary>
    /// 目录检索
    /// </summary>
    [XmlRoot("Query")]
    public class CatalogQuery : XmlHelper<CatalogQuery>
    {
        private static CatalogQuery _instance;
        /// <summary>
        /// 单例模式访问
        /// </summary>
        public static CatalogQuery Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CatalogQuery();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 命令类型
        /// </summary>
        [XmlElement("CmdType")]
        public CommandType CommandType { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [XmlElement("SN")]
        public int SN { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        [XmlElement("DeviceID")]
        public string DeviceID { get; set; }
    }
}
