using System.Collections;
using System.Collections.Generic;

namespace CLIP
{
    namespace Kernel
    {
        public static class Custom_Event
        {
            public delegate void Callback_STR(string str);
            public class Custom_Event_STR
            {
                public Callback_STR callback;
                public void add_callback_str(Callback_STR callback_neo)
                {
                    this.callback += callback_neo;
                }
                public void Invoke(string str)
                {
                    if (callback != null)
                    {
                        callback(str);
                    }
                }
            }
        }
    }
}