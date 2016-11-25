using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.GB28181.SIP.App
{
    [Table(Name = "NvrItem")]
    [DataContractAttribute]
    public class NvrItem : INotifyPropertyChanged, ISIPAsset
    {
        private List<ChannelItem> _channelItems = new List<ChannelItem>();

        public List<ChannelItem> Items
        {
            get { return _channelItems; }
        }

        public void Add(ChannelItem channel)
        {
            _channelItems.Add(channel);
        }


        private int _nvrID;

        public int NvrID
        {
            get { return _nvrID; }
            set { _nvrID = value; }
        }
        private string _nvrName;

        public string NvrName
        {
            get { return _nvrName; }
            set { _nvrName = value; }
        }

        private string _camID;

        public string CamID
        {
            get { return _camID; }
            set { _camID = value; }
        }


        private string _campIP;

        public string CampIP
        {
            get { return _campIP; }
            set { _campIP = value; }
        }
        private int _camPort;

        public int CamPort
        {
            get { return _camPort; }
            set { _camPort = value; }
        }
        private string _camUser;

        public string CamUser
        {
            get { return _camUser; }
            set { _camUser = value; }
        }
        private string _camPassword;

        public string CamPassword
        {
            get { return _camPassword; }
            set { _camPassword = value; }
        }
        private string _devType;

        public string DevType
        {
            get { return _devType; }
            set { _devType = value; }
        }
        private string _onvifAddress;

        public string OnvifAddress
        {
            get { return _onvifAddress; }
            set { _onvifAddress = value; }
        }
        private int _isAnalyzer;

        public int IsAnalyzer
        {
            get { return _isAnalyzer; }
            set { _isAnalyzer = value; }
        }
        private int _isBackRecord;

        public int IsBackRecord
        {
            get { return _isBackRecord; }
            set { _isBackRecord = value; }
        }
        private string _localID;

        public string LocalID
        {
            get { return _localID; }
            set { _localID = value; }
        }
        private string _localIP;

        public string LocalIP
        {
            get { return _localIP; }
            set { _localIP = value; }
        }
        private int _localPort;

        public int LocalPort
        {
            get { return _localPort; }
            set { _localPort = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public void Load(System.Data.DataRow row)
        {
            _nvrID = row["NvrID"] != null ? Convert.ToInt32(row["NvrID"]) : 0;
            _nvrName = row["NvrName"] as string;
            _camID = row["CamID"] as string;
            _campIP = row["CamIP"] as string;
            _camPort = row["CamPort"] != null ? Convert.ToInt32(row["CamPort"]) : 0;
            _camUser = row["CamUser"] as string;
            _camPassword = row["CamPassword"] as string;
            _devType = row["DevType"] as string;
            _onvifAddress = row["OnvifAddress"] as string;
            _isAnalyzer = row["IsAnalyzer"] != null ? Convert.ToInt32(row["IsAnalyzer"]) : 0;
            _isBackRecord = row["IsBackRecord"] != null ? Convert.ToInt32(row["IsBackRecord"]) : 0;
            _localID = row["LocalID"] as string;
            _localIP = row["LocalIP"] as string;
            _localPort = row["LocalPort"] != null ? Convert.ToInt32(row["LocalPort"]) : 0;
        }

        public System.Data.DataTable GetTable()
        {
            throw new NotImplementedException();
        }

        public string ToXML()
        {
            throw new NotImplementedException();
        }

        public string ToXMLNoParent()
        {
            throw new NotImplementedException();
        }

        public string GetXMLElementName()
        {
            throw new NotImplementedException();
        }

        public string GetXMLDocumentElementName()
        {
            throw new NotImplementedException();
        }
    }
}
