
namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Photo_Info
    {
        /// <summary>
        /// 照片id
        /// </summary>
        public int photo_id;

        /// <summary>
        /// 照片名
        /// </summary>
        public string photo_name;

        /// <summary>
        /// 稀有度
        /// </summary>
        public int rarity;

        /// <summary>
        /// 朋友名
        /// </summary>
        public string friend_name;

        /// <summary>
        /// 相机在场景中的机位ID，
        /// 一般按照顺序一个场景中1-20即可
        /// </summary>
        public int camera_id;

        /// <summary>
        /// 主角Pose名
        /// </summary>
        public string main_character_pose_name;

        /// <summary>
        /// 伙伴Pose名
        /// </summary>
        public string friend_character_pose_name;

        /// <summary>
        /// 特效名称
        /// </summary>
        public string fx_name;

        /// <summary>
        /// 后处理名称
        /// </summary>
        // public   string pp_name;
        public string sceneName;

        public override string ToString()
        {
            return "{ "
            + "photo_id:" + photo_id + ","
            + "photo_name:" + photo_name + ","
            + "friend_name:" + friend_name + ","
            + "main_character_pose_name:" + main_character_pose_name + ","
            + "friend_character_pose_name:" + friend_character_pose_name + ","
            + "fx_name:" + fx_name + ","
              //+ "pp_name:" + pp_name + ","
            + "}";
        }
    }
}