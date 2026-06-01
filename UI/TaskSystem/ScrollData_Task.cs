using System;
using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.UI
{
    public class ScrollData_Task
    {
        public MissionStaticData model;
        public MissionRuntimeData runtime;
        public bool isLocked;

        public ScrollData_Task(MissionStaticData model, MissionRuntimeData runtime)
        {
            this.model = model;
            this.runtime = runtime;
            isLocked = false;
        }

        public ScrollData_Task(MissionStaticData model, bool isLocked)
        {
            this.model = model;
            this.runtime = null;
            this.isLocked = isLocked;
        }
    }
}
