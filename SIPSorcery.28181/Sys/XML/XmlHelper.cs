using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SIPSorcery.Sys.XML
{
    public abstract class XmlHelper<T> where T : class
    {
        private static ILog logger = AppState.logger;

        /// <summary>
        /// 存储对象
        /// </summary>
        private T t;

        public XmlHelper() { }
        /// <summary>
        /// 序列化
        /// </summary>
        private void Serialize(T t)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            XmlSerializer s = new XmlSerializer(t.GetType());
            var xns = new XmlSerializerNamespaces();
            xns.Add("", "");
            XmlWriter w = XmlWriter.Create("c:\\catalog.xml",settings);
            s.Serialize(w, t, xns);
            w.Close();
            TextReader r = new StreamReader("c:\\catalog.xml");
            string xmlBody = r.ReadToEnd();
        }

        public string Serialize<T>(T obj)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.GetEncoding("GB2312");
            settings.NewLineOnAttributes = true;
            settings.OmitXmlDeclaration = false;
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                //去除默认命名空间
                xs.Serialize(writer, obj, xns);
            }
            return Encoding.UTF8.GetString(stream.ToArray()).Replace("\r", "");
        }

        ///// <summary>  
        ///// 对象序列化成 XML String  
        ///// </summary>  
        //public string Serialize<T>(T obj)
        //{
        //    string xmlString = string.Empty;
        //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        xmlSerializer.Serialize(ms, obj);
        //        xmlString = Encoding.UTF8.GetString(ms.ToArray());
        //    }
        //    return xmlString;
        //}  

        //public string Serialize<T>(T entity)
        //{
        //    StringBuilder buffer = new StringBuilder();

        //    XmlSerializer serializer = new XmlSerializer(typeof(T));
        //    using (TextWriter writer = new StringWriter(buffer))
        //    {
        //        serializer.Serialize(writer, entity);
        //    }

        //    return buffer.ToString();

        //}  

        ///// <summary>
        ///// 序列化
        ///// </summary>
        ///// <typeparam name="T">类型</typeparam>
        ///// <param name="entity">实体类型</param>
        ///// <returns>XML格式字符串</returns>
        //public string Serialize<T>(T entity)
        //{
        //    //StringBuilder 
        //    MemoryStream stream = new MemoryStream();
        //    XmlSerializer serializer = new XmlSerializer(typeof(T));
        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.Indent = true;
        //    settings.Encoding = new UTF8Encoding(false);
        //    settings.NewLineOnAttributes = true;
        //    settings.OmitXmlDeclaration = false;
        //    try
        //    {
        //        using (XmlWriter write = XmlWriter.Create(stream, settings))
        //        {
        //            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //            //去除默认命名空间
        //            ns.Add("", "");
        //            serializer.Serialize(stream, entity, ns);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("对象序列化到XML格式字符串错误" + ex.Message+ex.StackTrace.ToString());
        //    }
        //    stream.Close();
        //    return Encoding.UTF8.GetString(stream.ToArray()).Replace("\r", "");
        //} 

        /// <summary>
        /// 反序列
        /// </summary>
        /// <returns></returns>
        private T Deserialize(string xmlBody)
        {
            MemoryStream stream = new MemoryStream(Encoding.GetEncoding("GB2312").GetBytes(xmlBody));
            StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("GB2312"));

            //TextReader sr = new StringReader(xmlBody);
            XmlSerializer s = new XmlSerializer(typeof(T));
            object obj;
            try
            {
                obj = (T)s.Deserialize(sr);
            }
            catch (Exception ex)
            {
                logger.Error("反序列化错误" + ex.Message + ex.StackTrace.ToString());
                sr.Close();
                return null;
            }
            if (obj is T)
                t = obj as T;
            sr.Close();
            return t;
        }

        /// <summary>
        /// 读取文件并返回并构建成类
        /// </summary>
        /// <param name="xmlBody">XML文档</param>
        /// <returns>需要返回的类型格式</returns>
        public virtual T Read(string xmlBody)
        {
            return this.Deserialize(xmlBody);
        }

        public virtual void Save(T t)
        {
            this.Serialize(t);
        }

        public virtual string Save<T>(T t)
        {
            return this.Serialize<T>(t);
        }
    }
}
