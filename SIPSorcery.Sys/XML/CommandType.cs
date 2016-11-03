using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPSorcery.Sys.XML
{
    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 心跳
        /// </summary>
        Keepalive = 1,
        /// <summary>
        /// 设备目录
        /// </summary>
        Catalog = 2,
        /// <summary>
        /// 视频直播
        /// </summary>
        Play = 3,
        /// <summary>
        /// 录像点播
        /// </summary>
        Playback = 4
    }
}
