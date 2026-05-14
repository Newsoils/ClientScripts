using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            namespace Social
            {
                [System.Serializable]

                public class Social_Chat_Msg
                {
                    /// <summary>
                    /// 消息id
                    /// </summary>
                    public int msg_id;
                    /// <summary>
                    /// 消息名称
                    /// </summary>
                    public string msg_name;
                    /// <summary>
                    /// 颜文字符号
                    /// </ >
                    public string msg_symbol;
                    /// <summary>
                    /// 可能的表情包美术资源路径（和msg_symbol 互斥）
                    /// </summary>
                    public string res_url;

                    public override string ToString()
                    {
                        return "{ "
                        + "msg_id:" + msg_id + ","
                        + "msg_name:" + msg_name + ","
                        + "msg_symbol:" + msg_symbol + ","
                        + "res_url:" + res_url + ","
                        + "}";
                    }
                }
            }
        }
    }
}