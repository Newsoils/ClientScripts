namespace CLIP.Framework_Unity
{
    public interface ISystem
    {
        string Name { get; }

        /// <summary>系统初始化（在启动时调用）</summary>
        void OnInit();

        /// <summary>系统开始运行（场景加载完毕后调用）</summary>
        void OnStart();

        /// <summary>系统帧更新</summary>
        void OnUpdate(float deltaTime);

        /// <summary>系统销毁</summary>
        void OnDestroy();
    }
}