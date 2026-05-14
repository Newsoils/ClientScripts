using System;
using System.Collections;
using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel
{
    public class ServerTask
    {
        public bool isSendToServer;
        public string data;
        public Action<string, ServerTask> onReceiveMsg;
        public bool isBreak;
        public string result;
        public ServerTask(string data, Action<string, ServerTask> onReceiveMsg)
        {
            this.isSendToServer = true;
            this.data = data;
            this.onReceiveMsg = onReceiveMsg;
        }
        public ServerTask(Action<string, ServerTask> onReceiveMsg)
        {
            this.isSendToServer = false;
            this.onReceiveMsg = onReceiveMsg;
        }
    }
  
}

