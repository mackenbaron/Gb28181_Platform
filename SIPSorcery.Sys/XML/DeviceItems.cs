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
    [XmlRoot("Action")]
    public class DeviceItems : XmlHelper<DeviceItems>
    {
        private static DeviceItems _instance;

        public static DeviceItems Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceItems();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 目录
        /// </summary>
        [XmlElement("Query")]
        public Query ItemList { get; set; }
        /// <summary>
        /// 查询目录
        /// </summary>
        public class Query
        {
            /// <summary>
            /// 指令(ItemList)
            /// </summary>
            [XmlElement]
            public VariableType Variable { get; set; }
            /// <summary>
            /// 权限功能码
            /// </summary>
            [XmlElement("Privilege")]
            public int Privilege { get; set; }
            /// <summary>
            /// 地址编码
            /// </summary>
            [XmlElement("Address")]
            public string Address { get; set; }
            /// <summary>
            /// 开始页
            /// </summary>
            [XmlElement("FromIndex")]
            public int FromIndex { get; set; }
            /// <summary>
            /// 结束页
            /// </summary>
            [XmlElement("ToIndex")]
            public int ToIndex { get; set; }
        }
    }

    /// <summary>
    /// 设备目录查询结果
    /// </summary>
    [XmlRoot("Response")]
    public class DeviceItemsRes:XmlHelper<DeviceItemsRes>
    {
        private static DeviceItemsRes _instance;
        public static DeviceItemsRes Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceItemsRes();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 查询响应结果
        /// </summary>
        [XmlElement("QueryResponse")]
        public QueryResponse Query { get; set; }


        public class QueryResponse
        {
            /// <summary>
            /// 指令(ItemList)
            /// </summary>
            [XmlElement("Variable")]
            public VariableType Variable { get; set; }

            /// <summary>
            /// 地址编码
            /// </summary>
            [XmlElement("Parent")]
            public string Parent { get; set; }
            /// <summary>
            /// 目录下总共有多少
            /// </summary>
            [XmlElement("TotalSubNum")]
            public int TotalSubNum { get; set; }
            /// <summary>
            /// 总共有多少在线
            /// </summary>
            [XmlElement("TotalOnlineSubNum")]
            public int TotalOnlineSubNum { get; set; }
            /// <summary>
            /// 开始页
            [XmlElement("FromIndex")]
            public int FromIndex { get; set; }
            /// <summary>
            /// 结束页
            /// </summary>
            [XmlElement("ToIndex")]
            public int ToIndex { get; set; }
            /// <summary>
            /// 数量
            /// </summary>
            [XmlElement("SubNum")]
            public int SubNum { get; set; }
            /// <summary>
            /// 列表
            /// </summary>
            [XmlElement("SubList")]
            public SubList SubListItem { get; set; }
        }

        public class SubList
        {
            private List<Item> _items = new List<Item>();
            /// <summary>
            /// 项目
            /// </summary>
             [XmlElement("Item")]
            public List<Item> Items
            {
                get { return _items; }
                set { _items = value; }
            }

        }

        /// <summary>
        /// 列表项
        /// </summary>
        public class Item
        {
            /// <summary>
            /// 显示名
            /// </summary>
            [XmlElement("Name")]
            public string Name { get; set; }

            /// <summary>
            /// 地址编码
            /// </summary>
            [XmlElement("Address")]
            public string Address { get; set; }

            /// <summary>
            /// 类型
            /// </summary>
            [XmlElement("ResType")]
            public int RType { get; set; }

            /// <summary>
            /// 子类型
            /// </summary>
            [XmlElement("ResSubType")]
            public int RSubType { get; set; }

            /// <summary>
            /// 权限功能码
            /// </summary>
            [XmlElement("Privilege")]
            public int Privilege { get; set; }

            /// <summary>
            /// 活动状态
            /// </summary>
            [XmlElement("Status")]
            public int State { get; set; }

            /// <summary>
            /// 经度
            /// </summary>
            [XmlElement("Longitude")]
            public double Longitude { get; set; }

            /// <summary>
            /// 纬度
            /// </summary>
            [XmlElement("Latitude")]
            public double Latitude { get; set; }

            /// <summary>
            /// 海拔高度
            /// </summary>
            [XmlElement("Elevation")]
            public double Elevation { get; set; }

            /// <summary>
            /// 道路名称
            /// </summary>
            [XmlElement("Roadway")]
            public string Roadway { get; set; }

            /// <summary>
            /// 位置桩号
            /// </summary>
            [XmlElement("PileNo")]
            public int PileNo { get; set; }

            /// <summary>
            /// 区域编号
            /// </summary>
            [XmlElement("AreaNo")]
            public int AreaNo { get; set; }

            /// <summary>
            /// 更新时间 格式： YYYYMMDDTHHMMSSZ
            /// </summary>
            [XmlElement("UpdateTime")]
            public string UpdateTime { get; set; }
        }
    }
}
