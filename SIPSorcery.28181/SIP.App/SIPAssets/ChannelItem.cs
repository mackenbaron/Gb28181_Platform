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

    [Table(Name = "NvrChannel")]
    [DataContractAttribute]
    public class ChannelItem : INotifyPropertyChanged, ISIPAsset
    {

        private int _guid;

        public int Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private int _nvrID;

        public int NvrID
        {
            get { return _nvrID; }
            set { _nvrID = value; }
        }
        private int _channelID;

        public int ChannelID
        {
            get { return _channelID; }
            set { _channelID = value; }
        }
        private string _channelName;

        public string ChannelName
        {
            get { return _channelName; }
            set { _channelName = value; }
        }
        private int _frameRate;

        public int FrameRate
        {
            get { return _frameRate; }
            set { _frameRate = value; }
        }
        private string _streamFormat;

        public string StreamFormat
        {
            get { return _streamFormat; }
            set { _streamFormat = value; }
        }
        private string _audioFormat;

        public string AudioFormat
        {
            get { return _audioFormat; }
            set { _audioFormat = value; }
        }
        private string _rtsp1;

        public string Rtsp1
        {
            get { return _rtsp1; }
            set { _rtsp1 = value; }
        }
        private string _rtsp2;

        public string Rtsp2
        {
            get { return _rtsp2; }
            set { _rtsp2 = value; }
        }
        private string _mainResolution;

        public string MainResolution
        {
            get { return _mainResolution; }
            set { _mainResolution = value; }
        }
        private string _subResolution;

        public string SubResolution
        {
            get { return _subResolution; }
            set { _subResolution = value; }
        }
        private string _streamType;

        public string StreamType
        {
            get { return _streamType; }
            set { _streamType = value; }
        }
        private string _cameraID;

        public string CameraID
        {
            get { return _cameraID; }
            set { _cameraID = value; }
        }
        private string _areaName;

        public string AreaName
        {
            get { return _areaName; }
            set { _areaName = value; }
        }
        private int _isBackRecord;

        public int IsBackRecord
        {
            get { return _isBackRecord; }
            set { _isBackRecord = value; }
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
            _guid = row["Guid"] != null ? Convert.ToInt32(row["Guid"]) : 0;
            _nvrID = row["NvrID"] != null ? Convert.ToInt16(row["NvrID"]) : 0;
            _channelID = row["ChannelID"] != null ? Convert.ToInt16(row["ChannelID"]) : 0;
            _channelName = row["ChannelName"] as string;
            _frameRate = row["FrameRate"] != null ? Convert.ToInt16(row["FrameRate"]) : 0;
            _streamFormat = row["StreamFormat"] as string;
            _audioFormat = row["AudioFormat"] as string;
            _rtsp1 = row["Rtsp1"] as string;
            _rtsp2 = row["Rtsp2"] as string;
            _mainResolution = row["MainResolution"] as string;
            _subResolution = row["SubResolution"] as string;
            _streamType = row["StreamType"] as string;
            _cameraID = row["CameraID"] as string;
            _areaName = row["AreaName"] as string;
            _isBackRecord = row["IsBackRecord"] != null ? Convert.ToInt16(row["IsBackRecord"]) : 0;

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
