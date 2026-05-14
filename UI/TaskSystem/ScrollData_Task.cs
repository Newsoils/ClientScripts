using System;
using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.UI
{
    public class ScrollData_Task
    {
        public TaskModel model;
        public Task_RuntimeData runtime;

        public ScrollData_Task(TaskModel model, Task_RuntimeData runtime)
        {
            this.model = model;
            this.runtime = runtime;
        }
    }
}
