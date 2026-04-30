// 屏蔽词管理器
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {

            public class SensitiveWordManager : MonoBehaviour
            {
                public static SensitiveWordManager Instance { get; private set; }

                // 屏蔽词树节点
                private class TrieNode
                {
                    public Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
                    public bool IsEnd { get; set; }
                }

                private TrieNode root;
                private HashSet<string> sensitiveWords = new HashSet<string>();

                [Header("配置")]
                [Tooltip("可在 Inspector 额外挂词库（小），主词库见 Resources 下资源")]
                [SerializeField] private TextAsset defaultWordList;
                [Tooltip("Resources 内路径，无扩展名，如 Assets/Resources/SensitiveWords.txt 填 \"SensitiveWords\"")]
                [SerializeField] private string resourceWordListPath = "SensitiveWords";
                [Tooltip("运行时通过 Add 追加的词写入 persistentDataPath，可读写")]
                [SerializeField] private string userOverrideWordsFileName = "SensitiveWords_user.txt";
                [SerializeField] private char replacementChar = '*';

                [Header("匹配模式")]
                [SerializeField] private bool ignoreCase = true;
                [SerializeField] private bool matchFullWidth = true; // 匹配全角字符
                [SerializeField] private bool matchSimilarChars = true; // 匹配形似字

                // 相似字符映射
                private static readonly Dictionary<char, List<char>> similarChars = new Dictionary<char, List<char>>
                {
                    {'a', new List<char>{'@', 'ａ'}},
                    {'b', new List<char>{'ｂ', '6'}},
                    {'i', new List<char>{'1', 'ｉ', '!'}},
                    {'o', new List<char>{'0', 'ｏ', '○'}},
                    {'s', new List<char>{'5', 'ｓ', '$'}},
                    // 中文相似字可以继续添加
                };

                void Awake()
                {
                    if (Instance == null)
                    {
                        Instance = this;
                        DontDestroyOnLoad(gameObject);
                        Initialize();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }

                private void Initialize()
                {
                    root = new TrieNode();
                    LoadDefaultWords();
                    LoadCustomWords();
                    BuildTrie();
                }

                private void LoadDefaultWords()
                {
                    if (defaultWordList == null) return;
                    AddWordsFromLineText(defaultWordList.text, skipNumberSignComments: false);
                }

                /// <summary> 主词库 + 可读写区：Resources（全平台可加载）+ persistentDataPath 用户追加。 </summary>
                private void LoadCustomWords()
                {
                    if (!string.IsNullOrEmpty(resourceWordListPath))
                    {
                        var res = Resources.Load<TextAsset>(resourceWordListPath);
                        if (res == null)
                            Debug.LogError($"[SensitiveWordManager] 未在 Resources 找到词库: \"{resourceWordListPath}\" (相对 Resources，无 .txt 后缀)。");
                        else
                            AddWordsFromLineText(res.text, skipNumberSignComments: true);
                    }

                    string userPath = Path.Combine(Application.persistentDataPath, userOverrideWordsFileName);
                    if (!File.Exists(userPath)) return;
                    try
                    {
                        string text = File.ReadAllText(userPath);
                        AddWordsFromLineText(text, skipNumberSignComments: true);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[SensitiveWordManager] 读用户词库失败: {e.Message}");
                    }
                }

                private void AddWordsFromLineText(string text, bool skipNumberSignComments)
                {
                    if (string.IsNullOrEmpty(text)) return;
                    var lines = text.Split('\n');
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;
                        if (skipNumberSignComments && trimmed.StartsWith("#")) continue;
                        sensitiveWords.Add(trimmed);
                    }
                }

                private void BuildTrie()
                {
                    foreach (string word in sensitiveWords)
                    {
                        AddWordToTrie(word);
                    }
                }

                private void AddWordToTrie(string word)
                {
                    TrieNode node = root;
                    string processedWord = PreprocessWord(word);

                    foreach (char c in processedWord)
                    {
                        if (!node.Children.ContainsKey(c))
                        {
                            node.Children[c] = new TrieNode();
                        }
                        node = node.Children[c];
                    }
                    node.IsEnd = true;
                }

                private string PreprocessWord(string word)
                {
                    string result = word;

                    if (ignoreCase)
                    {
                        result = result.ToLower();
                    }

                    if (matchFullWidth)
                    {
                        result = ConvertToHalfWidth(result);
                    }

                    return result;
                }

                private string ConvertToHalfWidth(string input)
                {
                    // 全角转半角
                    char[] chars = input.ToCharArray();
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (chars[i] == '　')
                            chars[i] = ' ';
                        else if (chars[i] >= '！' && chars[i] <= '～')
                            chars[i] = (char)(chars[i] - 0xFEE0);
                    }
                    return new string(chars);
                }

                // 检查是否包含屏蔽词
                public bool ContainsSensitiveWords(string text)
                {
                    string processedText = PreprocessWord(text);
                    return FindSensitiveWords(processedText).Count > 0;
                }

                // 查找所有屏蔽词
                public List<string> FindSensitiveWords(string text)
                {
                    List<string> foundWords = new List<string>();
                    string processedText = PreprocessWord(text);

                    for (int i = 0; i < processedText.Length; i++)
                    {
                        TrieNode node = root;
                        int j = i;

                        while (j < processedText.Length && node.Children.ContainsKey(processedText[j]))
                        {
                            node = node.Children[processedText[j]];
                            if (node.IsEnd)
                            {
                                string foundWord = text.Substring(i, j - i + 1);
                                foundWords.Add(foundWord);
                            }
                            j++;
                        }
                    }

                    return foundWords;
                }

                // 过滤文本
                public string FilterText(string text)
                {
                    if (string.IsNullOrEmpty(text)) return text;

                    string processedText = PreprocessWord(text);
                    char[] result = text.ToCharArray();
                    bool[] shouldReplace = new bool[text.Length];

                    for (int i = 0; i < processedText.Length; i++)
                    {
                        TrieNode node = root;
                        int j = i;

                        while (j < processedText.Length)
                        {
                            char currentChar = processedText[j];

                            // 检查相似字符
                            List<char> alternatives = new List<char> { currentChar };
                            if (matchSimilarChars && similarChars.ContainsKey(currentChar))
                            {
                                alternatives.AddRange(similarChars[currentChar]);
                            }

                            TrieNode nextNode = null;
                            foreach (char alt in alternatives)
                            {
                                if (node.Children.ContainsKey(alt))
                                {
                                    nextNode = node.Children[alt];
                                    break;
                                }
                            }

                            if (nextNode == null) break;

                            node = nextNode;
                            if (node.IsEnd)
                            {
                                // 标记需要替换的字符
                                for (int k = i; k <= j && k < result.Length; k++)
                                {
                                    shouldReplace[k] = true;
                                }
                            }
                            j++;
                        }
                    }

                    // 执行替换
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (shouldReplace[i])
                        {
                            result[i] = replacementChar;
                        }
                    }

                    return new string(result);
                }

                // 添加新屏蔽词
                public bool AddSensitiveWord(string word)
                {
                    if (string.IsNullOrEmpty(word)) return false;

                    string trimmed = word.Trim();
                    if (sensitiveWords.Add(trimmed))
                    {
                        AddWordToTrie(trimmed);
                        SaveCustomWord(trimmed);
                        return true;
                    }
                    return false;
                }

                // 移除屏蔽词
                public bool RemoveSensitiveWord(string word)
                {
                    if (sensitiveWords.Remove(word))
                    {
                        // 重建Trie（简化实现，实际可以优化）
                        root = new TrieNode();
                        BuildTrie();
                        RemoveCustomWord(word);
                        return true;
                    }
                    return false;
                }

                private void SaveCustomWord(string word)
                {
                    string path = Path.Combine(Application.persistentDataPath, userOverrideWordsFileName);
                    string directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    File.AppendAllText(path, word + "\n");
                }

                private void RemoveCustomWord(string word)
                {
                    string path = Path.Combine(Application.persistentDataPath, userOverrideWordsFileName);
                    if (!File.Exists(path)) return;
                    var lines = File.ReadAllLines(path);
                    var newLines = new List<string>();
                    foreach (string line in lines)
                    {
                        if (line.Trim() != word) newLines.Add(line);
                    }
                    File.WriteAllLines(path, newLines.ToArray());
                }

                // 获取所有屏蔽词
                public List<string> GetAllSensitiveWords()
                {
                    return new List<string>(sensitiveWords);
                }

                // 清除所有屏蔽词
                public void ClearAllWords()
                {
                    sensitiveWords.Clear();
                    root = new TrieNode();
                }
            }
        }
    }
}

