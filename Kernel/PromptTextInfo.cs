using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 弹窗文本数据（对应 project_mouse_tb_prompt_text.json）。
    /// </summary>
    public class PromptTextInfo
    {
        [JsonProperty("promptId")]
        public int PromptId;

        [JsonProperty("promptName")]
        public string PromptName;

        [JsonProperty("promptDesc")]
        public string PromptDesc;

        [JsonProperty("promptType")]
        public string PromptType;

        [JsonProperty("cn")]
        public string Cn;

        [JsonProperty("en")]
        public string En;

        [JsonProperty("jp")]
        public string Jp;

        [JsonProperty("kr")]
        public string Kr;
    }
}
