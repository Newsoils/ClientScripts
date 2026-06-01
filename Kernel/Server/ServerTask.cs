using System;

using Google.Protobuf;

namespace CLIP.Project_Mouse.Kernel
{
    public class ServerTask
    {
        public IMessage data;
        public Action<string, ServerTask> onReceiveMsg;
        public bool isBreak;
        public string result;
        public ServerTask(IMessage data, Action<string, ServerTask> onReceiveMsg)
        {
            this.data = data;
            this.onReceiveMsg = onReceiveMsg;
        }
    }
  
}

