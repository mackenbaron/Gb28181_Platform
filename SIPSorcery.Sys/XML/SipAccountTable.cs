using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// sip账户
    /// </summary>
    [XmlRoot("sipaccounts")]
    public class SipAccountTable : XmlHelper<SipAccountTable>
    {
        private static SipAccountTable _instance;

        public static SipAccountTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SipAccountTable();
                }
                return _instance;
            }
        }


        [XmlElement("sipaccount")]
        public sipaccount Account { get; set; }

        /// <summary>
        /// 账户信息
        /// </summary>
        public class sipaccount
        {
            /// <summary>
            /// 编号
            /// </summary>
            [XmlElement("id")]
            public string id { get; set; }
            /// <summary>
            /// 所有者
            /// </summary>
            [XmlElement("owner")]
            public string owner { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            [XmlElement("sipusername")]
            public string sipusername { get; set; }
            /// <summary>
            /// 密码
            /// </summary>
            [XmlElement("sippassword")]
            public string sippassword { get; set; }
            /// <summary>
            /// sip远程域
            /// </summary>
            [XmlElement("sipdomain")]
            public string sipdomain { get; set; }
            /// <summary>
            /// 本地sip编号
            /// </summary>
            [XmlElement("localSipId")]
            public string localSipId { get; set; }
            /// <summary>
            /// 远程sip编号
            /// </summary>
            [XmlElement("remoteSipId")]
            public string remoteSipId { get; set; }
            /// <summary>
            /// sip终结点
            /// </summary>
            [XmlElement("sipsocket")]
            public string sipsocket { get; set; }
            /// <summary>
            /// 用户代理
            /// </summary>
            [XmlElement("useragent")]
            public string useragent { get; set; }
        }

        public override void Save(SipAccountTable t)
        {
            base.Save(t);
        }
    }
}
