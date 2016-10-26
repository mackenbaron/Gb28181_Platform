using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 设备目录
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
        public VariableType CommandType { get; set; }

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

    ///// <summary>
    ///// 目录报文解析
    ///// </summary>
    //[XmlRoot("Action")]
    //public class Catalog : XmlHelper<Catalog>
    //{
    //    private static Catalog _instance;

    //    /// <summary>
    //    /// 以单例模式访问
    //    /// </summary>
    //    public static Catalog Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //                _instance = new Catalog();
    //            return _instance;
    //        }
    //    }

    //    /// <summary>
    //    /// 指令(Catalog)
    //    /// </summary>
    //    [XmlElement("Variable")]
    //    public VariableType Variable { get; set; }

    //    /// <summary>
    //    /// 地址编码
    //    /// </summary>
    //    [XmlElement("Parent")]
    //    public long Parent { get; set; }

    //    /// <summary>
    //    /// 目录下总共有多少
    //    /// </summary>
    //    [XmlElement("TotalSubNum")]
    //    public int TotalSubNum { get; set; }

    //    /// <summary>
    //    /// 总共有多少在线
    //    /// </summary>
    //    [XmlElement("TotalOnlineSubNum")]
    //    public int TotalOnlineSubNum { get; set; }

    //    /// <summary>
    //    /// 数量
    //    /// </summary>
    //    [XmlElement("SubNum")]
    //    public int SubNum { get; set; }

    //    /// <summary>
    //    /// 列表
    //    /// </summary>
    //    [XmlElement("SubList")]
    //    public SubList SubLists { get; set; }

    //    public class SubList
    //    {
    //        private List<Item> _item = new List<Item>();

    //        /// <summary>
    //        /// 项目
    //        /// </summary>
    //        [XmlElement("Item")]
    //        public List<Item> Items
    //        {
    //            get
    //            {
    //                return _item;
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 列表项
    //    /// </summary>
    //    public class Item
    //    {
    //        /// <summary>
    //        /// 显示名
    //        /// </summary>
    //        [XmlElement("Name")]
    //        public string Name { get; set; }

    //        /// <summary>
    //        /// 地址编码
    //        /// </summary>
    //        [XmlElement("Address")]
    //        public string Address { get; set; }

    //        /// <summary>
    //        /// 类型
    //        /// </summary>
    //        [XmlElement("ResType")]
    //        public int RType { get; set; }

    //        /// <summary>
    //        /// 子类型
    //        /// </summary>
    //        [XmlElement("ResSubType")]
    //        public int RSubType { get; set; }

    //        /// <summary>
    //        /// 权限功能码
    //        /// </summary>
    //        [XmlElement("Privilege")]
    //        public int Privilege { get; set; }

    //        /// <summary>
    //        /// 活动状态
    //        /// </summary>
    //        [XmlElement("Status")]
    //        public int State { get; set; }

    //        /// <summary>
    //        /// 经度
    //        /// </summary>
    //        [XmlElement("Longitude")]
    //        public double Longitude { get; set; }

    //        /// <summary>
    //        /// 纬度
    //        /// </summary>
    //        [XmlElement("Latitude")]
    //        public double Latitude { get; set; }

    //        /// <summary>
    //        /// 海拔高度
    //        /// </summary>
    //        [XmlElement("Elevation")]
    //        public double Elevation { get; set; }

    //        /// <summary>
    //        /// 道路名称
    //        /// </summary>
    //        [XmlElement("Roadway")]
    //        public string Roadway { get; set; }

    //        /// <summary>
    //        /// 位置桩号
    //        /// </summary>
    //        [XmlElement("PileNo")]
    //        public int PileNo { get; set; }

    //        /// <summary>
    //        /// 区域编号
    //        /// </summary>
    //        [XmlElement("AreaNo")]
    //        public int AreaNo { get; set; }

    //        /// <summary>
    //        /// 操作类型
    //        /// </summary>
    //        [XmlElement("OperateType")]
    //        public Operate OperateType { get; set; }

    //        /// <summary>
    //        /// 更新时间 格式： YYYYMMDDTHHMMSSZ
    //        /// </summary>
    //        [XmlElement("UpdateTime")]
    //        public string UpdateTime { get; set; }
    //    }
    //}

    ///// <summary>
    ///// 二级资源类型
    ///// </summary>
    //public enum ResSubType
    //{
    //    /// <summary>
    //    /// 可控标清球机(或带云台标清枪机)
    //    /// </summary>
    //    ControllableLowCamera = 0,
    //    /// <summary>
    //    /// 不可控标清球机(或不可控标清枪机)
    //    /// </summary>
    //    UnControllableLowCamera = 1,
    //    /// <summary>
    //    /// 可控高清球机(或带云台高清枪机)
    //    /// </summary>
    //    ControllableHighCamera = 2,
    //    /// <summary>
    //    /// 不可控高清球机(或不可控高清枪机)
    //    /// </summary>
    //    UnControllableHighCamera = 3,
    //    /// <summary>
    //    /// 移动监控
    //    /// </summary>
    //    MoveMonitor = 4,
    //    /// <summary>
    //    /// 其他
    //    /// </summary>
    //    Other = 5
    //}

    ///// <summary>
    ///// 资源类型
    ///// </summary>
    //public enum ResType
    //{
    //    /// <summary>
    //    /// 域节点(目录或组织)
    //    /// </summary>
    //    DomainNode = 0,
    //    /// <summary>
    //    /// 摄像机
    //    /// </summary>
    //    Camera = 1,
    //    /// <summary>
    //    /// 输入开关量
    //    /// </summary>
    //    InOnOffValue = 2,
    //    /// <summary>
    //    /// 输出开关量
    //    /// </summary>
    //    OutOnOffValue = 3
    //}

    ///// <summary>
    ///// 设备状态
    ///// </summary>
    //public enum Status
    //{
    //    /// <summary>
    //    /// 正常
    //    /// </summary>
    //    Normal = 0,
    //    /// <summary>
    //    /// 不正常
    //    /// </summary>
    //    Unusual = 1,
    //    /// <summary>
    //    /// 报修中
    //    /// </summary>
    //    Warranty = 2,
    //    /// <summary>
    //    /// 搬迁中
    //    /// </summary>
    //    Relocation = 3,
    //    /// <summary>
    //    /// 在建
    //    /// </summary>
    //    Build = 4,
    //    /// <summary>
    //    /// 断电
    //    /// </summary>
    //    NoElectric = 5
    //}

    ///// <summary>
    ///// 操作类型
    ///// </summary>
    //public enum Operate
    //{
    //    /// <summary>
    //    /// 添加共享
    //    /// </summary>
    //    ADD = 0,
    //    /// <summary>
    //    /// 取消共享
    //    /// </summary>
    //    DEL = 1,
    //    /// <summary>
    //    /// 修改共享
    //    /// </summary>
    //    MOD = 2,
    //    /// <summary>
    //    /// 保留
    //    /// </summary>
    //    OTH = 3
    //}



    //public enum CmdType
    //{
    //    Catalog = 0
    //}
}
