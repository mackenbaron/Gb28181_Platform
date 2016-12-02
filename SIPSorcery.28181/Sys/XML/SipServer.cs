using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
namespace SIPSorcery.GB28181.Sys.XML
{
    [XmlRoot("sipServer")]
    public class SipServer 
    {
        private static SipServer _instance;
        public static SipServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SipServer();
                }
                return _instance;
            }
        }

        public  void Save<T>(T t)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            //settings.Encoding = Encoding.GetEncoding("GB2312");
            //settings.Encoding = new UTF8Encoding(false);
            //settings.NewLineOnAttributes = true;
            //settings.OmitXmlDeclaration = false;
            string xml = AppDomain.CurrentDomain.BaseDirectory + "Config\\SipSocket.xml";
            using (XmlWriter writer = XmlWriter.Create(xml, settings))
            {
                var xns = new XmlSerializerNamespaces();

                xns.Add(string.Empty, string.Empty);
                //去除默认命名空间
                xs.Serialize(writer, t, xns);
                writer.Close();
                writer.Dispose();
            }
            //return Encoding.UTF8.GetString(stream.ToArray()).Replace("\r", "");
        }

        [XmlElement("sipsockets")]
        public SipSocket sipsockets { get; set; }

        [XmlElement("useragentconfigs")]
        public UserAgentConfig useragentconfigs { get; set; }


        public class SipSocket
        {
            [XmlElement("sipsocket")]
            public string sipsocket { get; set; }
        }

        public class UserAgentConfig
        {
            [XmlElement("useragent")]
            public Agent useragent { get; set; }
        }


        public class Agent
        {
            [XmlAttribute("agent")]
            public string agent { get; set; }
            [XmlAttribute("expiry")]
            public int expiry { get; set; }

            [XmlAttribute("contactlists")]
            public bool contactlists { get; set; }
        }
    }
}
