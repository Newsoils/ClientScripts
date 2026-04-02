using System.Text.RegularExpressions;

namespace CLIP.Framework_Core.Tools
{
    public static class StringTools
    {
        /// <summary>
        /// 去掉字符串末尾的数字部分（用于匹配语言包名，如 playmore-lg_zh1 → playmore-lg_zh）
        /// </summary>
        /// <param name="name">原始名称</param>
        /// <param name="splitChar">分隔符，如 '_' </param>
        /// <returns>去掉数字后的名称</returns>
        public static string GetNoNumberName(string name, char splitChar = '_')
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // 按分隔符拆分
            var parts = name.Split(splitChar);
            if (parts.Length == 0)
                return name;

            string last = parts[parts.Length - 1];
            // 检查最后一段是否是纯数字
            bool isNumeric = true;
            foreach (char c in last)
            {
                if (!char.IsDigit(c))
                {
                    isNumeric = false;
                    break;
                }
            }

            // 如果末尾不是纯数字，直接返回
            if (!isNumeric)
                return name;

            // 去掉最后一段数字
            return string.Join(splitChar.ToString(), parts, 0, parts.Length - 1);
        }




        /// <summary>
        /// 将字符串转化成C#合法的string
        /// 仅保留字母、数字、下划线;
        /// 可用于生成资源 Key、变量名或文件名
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="toLower">是否需要转化成小写</param>
        /// <returns>处理好的字符串，如果字符串不合法会返回"invalid"，需要进行特殊处理</returns>
        public static string ToSafeString(string input, bool toLower = false)
        {
            if (string.IsNullOrEmpty(input)) return "invalid";

            // 1. 转大小写（通常 Key 用小写，变量名用大写）
            string result = toLower ? input.ToLower() : input.ToUpper();

            // 2. 将非字母数字的字符（包括空格、&、%、-、.等）替换为下划线
            // [^a-zA-Z0-9] 表示匹配除这些以外的所有字符
            result = Regex.Replace(result, @"[^a-zA-Z0-9]", "_");

            // 3. 将连续的多个下划线合并为一个（如 "hero__run" -> "hero_run"）
            result = Regex.Replace(result, @"_+", "_");

            // 4. 去除首尾的下划线
            result = result.Trim('_');

            // 5. 如果首字符是数字，前面加个下划线（保证符合 C# 变量命名规范）
            if (result.Length > 0 && char.IsDigit(result[0]))
            {
                result = "_" + result;
            }

            return string.IsNullOrEmpty(result) ? "invalid" : result;
        }
    }
}
